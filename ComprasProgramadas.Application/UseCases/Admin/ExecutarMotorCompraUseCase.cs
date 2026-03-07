using System.Text.Json;
using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Admin;

/// <summary>
/// Motor de Compra Programada  executado no 5 dia util de cada mes.
///
/// Pipeline:
///   1. Consolida valor de todos os clientes ativos
///   2. Cria OrdemCompra e salva (precisa do Id)
///   3. Para cada ativo da cesta Top Five:
///      a. Busca cotacao de D-1
///      b. Calcula quantidade (TRUNCAR) descontando saldo Master
///      c. Distribui proporcionalmente entre clientes
///      d. Atualiza CustodiaFilhote de cada cliente (preco medio RN-042)
///      e. Acumula residuo de volta ao Master
///      f. Publica IR dedo-duro no Kafka (RN-044/RN-053)
///   4. Finaliza a ordem como Executada
/// </summary>
public class ExecutarMotorCompraUseCase
{
    private readonly IClienteRepository          _clienteRepo;
    private readonly ICestaTopFiveRepository     _cestaRepo;
    private readonly ICotacaoHistoricaRepository _cotacaoRepo;
    private readonly ICustodiaMasterRepository   _masterRepo;
    private readonly ICustodiaFilhoteRepository  _filhoteRepo;
    private readonly IContaGraficaRepository     _contaRepo;
    private readonly IOrdemCompraRepository      _ordemRepo;
    private readonly IDistribuicaoRepository     _distribuicaoRepo;
    private readonly IKafkaPublisher             _kafka;
    private readonly IUnitOfWork                 _uow;

    public ExecutarMotorCompraUseCase(
        IClienteRepository          clienteRepo,
        ICestaTopFiveRepository     cestaRepo,
        ICotacaoHistoricaRepository cotacaoRepo,
        ICustodiaMasterRepository   masterRepo,
        ICustodiaFilhoteRepository  filhoteRepo,
        IContaGraficaRepository     contaRepo,
        IOrdemCompraRepository      ordemRepo,
        IDistribuicaoRepository     distribuicaoRepo,
        IKafkaPublisher             kafka,
        IUnitOfWork                 uow)
    {
        _clienteRepo     = clienteRepo;
        _cestaRepo       = cestaRepo;
        _cotacaoRepo     = cotacaoRepo;
        _masterRepo      = masterRepo;
        _filhoteRepo     = filhoteRepo;
        _contaRepo       = contaRepo;
        _ordemRepo       = ordemRepo;
        _distribuicaoRepo = distribuicaoRepo;
        _kafka           = kafka;
        _uow             = uow;
    }

    public async Task<MotorCompraResponse> ExecutarAsync(ExecutarMotorCompraRequest request)
    {
        // ===== PASSO 1: Data e cesta ativa =====
        var dataReferencia = request.DataReferencia ?? DateOnly.FromDateTime(DateTime.Today);
        var dataFechamento = dataReferencia.AddDays(-1);

        var cesta = await _cestaRepo.ObterAtivaAsync()
            ?? throw new DomainException("Nao ha cesta Top Five ativa.");

        // ===== PASSO 2: Consolida valores de todos os clientes ativos =====
        var clientes = (await _clienteRepo.ListarAtivosAsync()).ToList();
        if (!clientes.Any())
            throw new DomainException("Nenhum cliente ativo para processar.");

        decimal totalConsolidado = clientes.Sum(c => c.ValorMensal);

        // ===== PASSO 3: Cria a OrdemCompra e salva para obter o Id =====
        var ordem = OrdemCompra.Criar(cesta.Id, dataReferencia, totalConsolidado, null);
        await _ordemRepo.AdicionarAsync(ordem);
        await _uow.CommitAsync(); // necessario para ter ordem.Id

        var itensResposta = new List<ItemMotorResponse>();

        // ===== PASSO 4: Processa cada ativo da cesta =====
        foreach (var itemCesta in cesta.Itens)
        {
            // 4a. Valor proporcional ao ativo (RN-023)
            decimal valorAlvo = totalConsolidado * (itemCesta.Percentual / 100m);

            // 4b. Cotacao de D-1 (RN-025/RN-027)
            var cotacao = await _cotacaoRepo.ObterPorTickerEDataAsync(itemCesta.Ticker, dataFechamento);
            if (cotacao is null)
                throw new DomainException($"Cotacao de {itemCesta.Ticker} para {dataFechamento} nao encontrada.");

            // 4c. Saldo residual na Master para este ticker
            var master = await _masterRepo.ObterPorTickerAsync(itemCesta.Ticker);
            int saldoMaster = master?.Quantidade ?? 0;

            // 4d. Calcular item da ordem (TRUNCAR, lote/fracionario  RN-026/RN-031/RN-032)
            var itemOrdem = ItemOrdemCompra.Calcular(
                ordemId: ordem.Id,
                ticker: itemCesta.Ticker,
                valorAlvo: valorAlvo,
                cotacaoFechamento: cotacao.PrecoFechamento,
                saldoMaster: saldoMaster);

            // 4e. Descontar saldo usado da Master
            if (itemOrdem.SaldoMasterDescontado > 0 && master is not null)
            {
                master.Descontar(itemOrdem.SaldoMasterDescontado);
                _masterRepo.Atualizar(master);
            }

            ordem.Itens.Add(itemOrdem);

            // ===== PASSO 5: Distribuir entre clientes (RN-034/RN-035/RN-036/RN-037) =====
            int totalDistribuido = 0;

            foreach (var cliente in clientes)
            {
                decimal proporcao = cliente.ValorMensal / totalConsolidado;

                // TRUNCAR a proporcao  nunca arredondar (RN-037)
                int qtdClientLote = (int)Math.Truncate(itemOrdem.QtdLotePadrao * proporcao);
                int qtdClientFrac = (int)Math.Truncate(itemOrdem.QtdFracionario * proporcao);

                if (qtdClientLote <= 0 && qtdClientFrac <= 0) continue;

                totalDistribuido += qtdClientLote + qtdClientFrac;

                // 5a. Atualiza custódia lote padrao
                if (qtdClientLote > 0)
                {
                    var cust = await ObterOuCriarCustodiaAsync(cliente.Id, itemCesta.Ticker);
                    cust.RegistrarCompra(qtdClientLote, cotacao.PrecoFechamento);
                    _filhoteRepo.Atualizar(cust);
                }

                // 5b. Atualiza custodia fracionaria (ticker + "F")
                if (qtdClientFrac > 0)
                {
                    var tickerFrac = itemCesta.Ticker + "F";
                    var custFrac = await ObterOuCriarCustodiaAsync(cliente.Id, tickerFrac);
                    custFrac.RegistrarCompra(qtdClientFrac, cotacao.PrecoFechamento);
                    _filhoteRepo.Atualizar(custFrac);
                }

                // 5c. Registrar distribuicao para auditoria (RN-040)
                var dist = Distribuicao.Criar(
                    ordemId: ordem.Id,
                    clienteId: cliente.Id,
                    ticker: itemCesta.Ticker,
                    quantidade: qtdClientLote + qtdClientFrac,
                    precoUnitario: cotacao.PrecoFechamento,
                    proporcaoCliente: proporcao);

                await _distribuicaoRepo.AdicionarAsync(dist);
            }

            // 4f. Acumula residuo de acao nao distribuida de volta ao Master (RN-039)
            int residuoQtd = itemOrdem.QuantidadeTotalDisponivel - totalDistribuido;
            if (residuoQtd > 0)
            {
                if (master is null)
                {
                    master = CustodiaMaster.Criar(itemCesta.Ticker);
                    await _masterRepo.AdicionarAsync(master);
                }
                master.AdicionarResiduo(residuoQtd, cotacao.PrecoFechamento);
                _masterRepo.Atualizar(master);
            }

            itensResposta.Add(new ItemMotorResponse(
                itemCesta.Ticker,
                Math.Round(valorAlvo, 2),
                cotacao.PrecoFechamento,
                itemOrdem.QuantidadeCalculada,
                itemOrdem.SaldoMasterDescontado,
                itemOrdem.QuantidadeAComprar,
                itemOrdem.QtdLotePadrao,
                itemOrdem.QtdFracionario));
        }

        // ===== PASSO 6: Finaliza ordem e salva tudo =====
        ordem.MarcarExecutada();
        _ordemRepo.Atualizar(ordem);
        await _uow.CommitAsync();

        // ===== PASSO 7: Publica IR dedo-duro no Kafka para cada distribuicao =====
        // (feito APOS commit para distribuicoes terem IDs reais)
        var distribuicoesPendentes = await _distribuicaoRepo.ListarNaoPublicadasKafkaAsync();
        foreach (var dist in distribuicoesPendentes)
        {
            var payload = JsonSerializer.Serialize(new
            {
                ClienteId = dist.ClienteId,
                Ticker    = dist.Ticker,
                Valor     = dist.ValorOperacao,
                Ir        = dist.ValorIrDedoDuro,
                Data      = dataReferencia
            });

            await _kafka.PublicarAsync("ir-dedo-duro", dist.ClienteId.ToString(), payload);
            dist.MarcarKafkaPublicado();
            _distribuicaoRepo.Atualizar(dist);
        }
        await _uow.CommitAsync();

        return new MotorCompraResponse(
            ordem.Id, dataReferencia,
            Math.Round(totalConsolidado, 2),
            clientes.Count,
            itensResposta,
            ordem.Status.ToString());
    }

    private async Task<CustodiaFilhote> ObterOuCriarCustodiaAsync(long clienteId, string ticker)
    {
        var cust = await _filhoteRepo.ObterPorClienteETickerAsync(clienteId, ticker);
        if (cust is not null) return cust;

        var conta = await _contaRepo.ObterPorClienteAsync(clienteId)
            ?? throw new DomainException($"Conta grafica nao encontrada para cliente {clienteId}.");

        cust = CustodiaFilhote.Criar(clienteId, conta.Id, ticker);
        await _filhoteRepo.AdicionarAsync(cust);
        return cust;
    }
}
