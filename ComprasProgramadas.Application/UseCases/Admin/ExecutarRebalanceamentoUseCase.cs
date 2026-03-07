using System.Text.Json;
using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Admin;

/// <summary>
/// Executa todos os rebalanceamentos pendentes.
///
/// Fluxo por cliente (RN-045 a RN-060):
///   1. Para cada ativo que SAIU da cesta → vende toda a posição
///   2. Para cada ativo que MUDOU de percentual → calcula desvio e vende/compra diferença
///   3. Para ativos NOVOS na cesta → compra com o valor obtido nas vendas
///   4. Se vendas do mês > R$ 20.000 → cobra 20% de IR sobre lucro líquido (RN-057/RN-058)
///   5. Publica IR no Kafka (RN-059)
/// </summary>
public class ExecutarRebalanceamentoUseCase
{
    private readonly IRebalanceamentoRepository  _rebalRepo;
    private readonly ICestaTopFiveRepository     _cestaRepo;
    private readonly ICustodiaFilhoteRepository  _filhoteRepo;
    private readonly ICotacaoHistoricaRepository _cotacaoRepo;
    private readonly IVendaMesRepository         _vendaMesRepo;
    private readonly IKafkaPublisher             _kafka;
    private readonly IUnitOfWork                 _uow;

    public ExecutarRebalanceamentoUseCase(
        IRebalanceamentoRepository  rebalRepo,
        ICestaTopFiveRepository     cestaRepo,
        ICustodiaFilhoteRepository  filhoteRepo,
        ICotacaoHistoricaRepository cotacaoRepo,
        IVendaMesRepository         vendaMesRepo,
        IKafkaPublisher             kafka,
        IUnitOfWork                 uow)
    {
        _rebalRepo    = rebalRepo;
        _cestaRepo    = cestaRepo;
        _filhoteRepo  = filhoteRepo;
        _cotacaoRepo  = cotacaoRepo;
        _vendaMesRepo = vendaMesRepo;
        _kafka        = kafka;
        _uow          = uow;
    }

    /// <summary>
    /// Executa todos os rebalanceamentos pendentes no banco.
    /// Pode ser chamado via endpoint ou pelo scheduler.
    /// </summary>
    public async Task<RebalanceamentoResponse> ExecutarAsync()
    {
        var pendentes = (await _rebalRepo.ListarPendentesAsync()).ToList();

        if (!pendentes.Any())
            return new RebalanceamentoResponse(0, 0, "Nenhum rebalanceamento pendente.");

        int totalExecutados = 0;
        int totalClientes   = 0;

        foreach (var reba in pendentes)
        {
            if (reba.CestaNova is null)
            {
                reba.MarcarErro();
                _rebalRepo.Atualizar(reba);
                await _uow.CommitAsync();
                continue;
            }

            var tickersNovaCesta = reba.CestaNova.Itens
                .ToDictionary(i => i.Ticker, i => i.Percentual);

            foreach (var rebaCli in reba.Clientes.Where(c => c.Status == StatusRebalanceamento.Pendente))
            {
                try
                {
                    var (vendas, compras, irDevido) = await ProcessarClienteAsync(
                        rebaCli.ClienteId, tickersNovaCesta);

                    rebaCli.RegistrarResultado(vendas, compras, irDevido);
                    _rebalRepo.AtualizarCliente(rebaCli);
                    totalClientes++;
                }
                catch (Exception ex)
                {
                    rebaCli.MarcarErro();
                    _rebalRepo.AtualizarCliente(rebaCli);
                    _ = ex; // log em produção
                }
            }

            reba.MarcarExecutado();
            _rebalRepo.Atualizar(reba);
            await _uow.CommitAsync();
            totalExecutados++;
        }

        return new RebalanceamentoResponse(totalExecutados, totalClientes,
            $"{totalExecutados} rebalanceamento(s) executado(s) para {totalClientes} cliente(s).");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LÓGICA POR CLIENTE
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<(decimal TotalVendas, decimal TotalCompras, decimal IrDevido)>
        ProcessarClienteAsync(long clienteId, Dictionary<string, decimal> tickersNovaCesta)
    {
        // 1. Busca custódias do cliente com posição aberta
        var custodias = (await _filhoteRepo.ListarPorClienteAsync(clienteId))
            .Where(c => c.Quantidade > 0)
            .ToList();

        if (!custodias.Any())
            return (0, 0, 0);

        decimal totalVendas = 0;
        decimal lucroLiquidoTotal = 0;

        // 2. Calcular valor total atual da carteira para determinar decomposição ideal
        decimal valorTotalCarteira = 0;
        var cotacoes = new Dictionary<string, decimal>(); // ticker → preço atual

        foreach (var cust in custodias)
        {
            // Ticker fracionário (PETR4F) → busca cotação do ativo base (PETR4)
            var tickerBase = cust.Ticker.EndsWith("F") && cust.Ticker.Length > 1
                ? cust.Ticker[..^1]
                : cust.Ticker;

            var cotacao = await _cotacaoRepo.ObterUltimaCotacaoAsync(tickerBase);
            var preco   = cotacao?.PrecoFechamento ?? 0m;
            cotacoes[cust.Ticker] = preco;
            valorTotalCarteira   += cust.Quantidade * preco;
        }

        // 3. Identificar tickers que SAÍRAM da cesta → vender tudo (RN-046/RN-047)
        foreach (var cust in custodias)
        {
            // Para ativo fracionário, verifica o ticker base na cesta
            var tickerBase = cust.Ticker.EndsWith("F") && cust.Ticker.Length > 1
                ? cust.Ticker[..^1]
                : cust.Ticker;

            if (!tickersNovaCesta.ContainsKey(tickerBase))
            {
                var precoVenda = cotacoes.GetValueOrDefault(cust.Ticker, 0m);
                if (precoVenda <= 0 || cust.Quantidade == 0) continue;

                // Captura quantidade E preço médio ANTES de RegistrarVenda (que zerará a qtd)
                var qtdVendida  = cust.Quantidade;
                var pmAnteVenda = cust.PrecoMedio;
                cust.RegistrarVenda(qtdVendida, precoVenda);

                // Registrar a venda para controle de IR mensal
                var venda  = VendaMes.Registrar(clienteId, cust.Ticker, qtdVendida, precoVenda,
                    pmAnteVenda, OrigemVenda.RebalanceamentoCesta);
                await _vendaMesRepo.AdicionarAsync(venda);

                totalVendas          += venda.ValorTotalVenda;
                lucroLiquidoTotal    += venda.Lucro;

                _filhoteRepo.Atualizar(cust);
            }
        }

        // 4. Rebalancear ativos que PERMANECERAM mas COM NOVA PROPORÇÃO (RN-049)
        //    Valor ideal de cada ativo = valorTotalCarteira × percentual_novo
        //    Se valor atual > valor ideal → vender excesso
        //    Se valor atual < valor ideal → comprar com caixa das vendas (simplificado)
        foreach (var (ticker, percentual) in tickersNovaCesta)
        {
            var cust = custodias.FirstOrDefault(c => c.Ticker == ticker);
            if (cust == null || cust.Quantidade == 0) continue;

            var precoAtual = cotacoes.GetValueOrDefault(ticker, 0m);
            if (precoAtual <= 0) continue;

            var valorAtual = cust.Quantidade * precoAtual;
            var valorIdeal = valorTotalCarteira * (percentual / 100m);

            // Excesso: vender ações a mais
            if (valorAtual > valorIdeal * 1.05m) // tolerância RN-051: >5%
            {
                var excessoValor = valorAtual - valorIdeal;
                var qtdVender    = (int)Math.Truncate(excessoValor / precoAtual);

                if (qtdVender > 0 && qtdVender <= cust.Quantidade)
                {
                    var lucro = cust.RegistrarVenda(qtdVender, precoAtual);
                    var venda = VendaMes.Registrar(clienteId, ticker, qtdVender, precoAtual,
                        cust.PrecoMedio, OrigemVenda.RebalanceamentoDesvio);
                    await _vendaMesRepo.AdicionarAsync(venda);

                    totalVendas       += venda.ValorTotalVenda;
                    lucroLiquidoTotal += venda.Lucro;

                    _filhoteRepo.Atualizar(cust);
                }
            }
        }

        await _uow.CommitAsync(); // commit vendas

        // 5. Calcular IR sobre vendas do mês (RN-057/RN-058)
        var mesReferencia  = DateTime.UtcNow.ToString("yyyy-MM");
        var totalVendasMes = await _vendaMesRepo.ObterTotalVendasMesAsync(clienteId, mesReferencia);
        var lucroMes       = await _vendaMesRepo.ObterLucroLiquidoMesAsync(clienteId, mesReferencia);

        decimal irDevido = 0;
        if (totalVendasMes > 20_000m && lucroMes > 0)
        {
            irDevido = Math.Round(lucroMes * 0.20m, 2); // RN-058: 20% sobre lucro líquido

            // Publicar IR no Kafka (RN-059)
            var payload = JsonSerializer.Serialize(new
            {
                ClienteId      = clienteId,
                TipoIr         = "IR_RENDA_VARIAVEL",
                TotalVendasMes = totalVendasMes,
                LucroLiquidoMes = lucroMes,
                IrDevido       = irDevido,
                MesReferencia  = mesReferencia,
                Data           = DateTime.UtcNow
            });

            await _kafka.PublicarAsync("ir-renda-variavel", clienteId.ToString(), payload);
        }

        // 6. Comprar ativos NOVOS na cesta com o caixa das vendas (RN-048)
        //    Simplificado: distribui o totalVendas proporcionalmente entre ativos novos
        var custodiasTickers = custodias.Select(c => c.Ticker).ToHashSet();
        var ativosNovos = tickersNovaCesta.Keys
            .Where(t => !custodiasTickers.Contains(t))
            .ToList();

        decimal totalCompras = 0;

        if (ativosNovos.Any() && totalVendas > 0)
        {
            var percentualTotalNovos = ativosNovos.Sum(t => tickersNovaCesta[t]);

            foreach (var ticker in ativosNovos)
            {
                var cotacao = await _cotacaoRepo.ObterUltimaCotacaoAsync(ticker);
                if (cotacao is null || cotacao.PrecoFechamento <= 0) continue;

                var percentualRelativo = tickersNovaCesta[ticker] / percentualTotalNovos;
                var valorDisponivel    = totalVendas * percentualRelativo;
                var qtdComprar         = (int)Math.Truncate(valorDisponivel / cotacao.PrecoFechamento);

                if (qtdComprar <= 0) continue;

                var custNova = await _filhoteRepo.ObterPorClienteETickerAsync(clienteId, ticker);
                if (custNova is null)
                {
                    // Cria a custódia apenas se o cliente já tiver conta gráfica
                    // (simplificado: assume que existe pois o cliente é ativo)
                    continue; // em produção: criar via ObterOuCriarCustodiaAsync
                }

                custNova.RegistrarCompra(qtdComprar, cotacao.PrecoFechamento);
                _filhoteRepo.Atualizar(custNova);
                totalCompras += qtdComprar * cotacao.PrecoFechamento;
            }

            await _uow.CommitAsync();
        }

        return (totalVendas, totalCompras, irDevido);
    }
}
