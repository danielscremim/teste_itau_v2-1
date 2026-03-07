using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Clientes;

/// <summary>
/// Retorna visão completa de rentabilidade da carteira do cliente.
///
/// Diferenças em relação ao ConsultarCarteiraUseCase:
///   - P/L percentual por ativo
///   - Comparação da composição real vs cesta recomendada (detecta desvios)
///   - Histórico mensal de distribuições recebidas (evolução do patrimônio)
///   - IR dedo-duro acumulado retido
///
/// Endpoint obrigatório do requisito 4.1:
///   "Tela/endpoint que exibe informações detalhadas de rentabilidade da carteira"
/// </summary>
public class ConsultarRentabilidadeUseCase
{
    private readonly IClienteRepository          _clienteRepo;
    private readonly ICustodiaFilhoteRepository  _custodiaRepo;
    private readonly ICotacaoHistoricaRepository _cotacaoRepo;
    private readonly ICestaTopFiveRepository     _cestaRepo;
    private readonly IDistribuicaoRepository     _distribuicaoRepo;

    public ConsultarRentabilidadeUseCase(
        IClienteRepository          clienteRepo,
        ICustodiaFilhoteRepository  custodiaRepo,
        ICotacaoHistoricaRepository cotacaoRepo,
        ICestaTopFiveRepository     cestaRepo,
        IDistribuicaoRepository     distribuicaoRepo)
    {
        _clienteRepo      = clienteRepo;
        _custodiaRepo     = custodiaRepo;
        _cotacaoRepo      = cotacaoRepo;
        _cestaRepo        = cestaRepo;
        _distribuicaoRepo = distribuicaoRepo;
    }

    public async Task<RentabilidadeResponse> ExecutarAsync(long clienteId)
    {
        // ── 1. Carrega dados base ─────────────────────────────────────────────
        var cliente = await _clienteRepo.ObterPorIdAsync(clienteId)
            ?? throw new DomainException($"Cliente {clienteId} não encontrado.");

        var custodias    = (await _custodiaRepo.ListarPorClienteAsync(clienteId)).ToList();
        var cestaAtiva   = await _cestaRepo.ObterAtivaAsync();
        var distribuicoes = (await _distribuicaoRepo.ListarPorClienteAsync(clienteId)).ToList();

        // ── 2. Calcula P/L por ativo ──────────────────────────────────────────
        var ativos       = new List<AtivoRentabilidadeResponse>();
        decimal totInv   = 0, totAtual = 0;

        foreach (var cust in custodias.Where(c => c.Quantidade > 0))
        {
            var cotacao       = await _cotacaoRepo.ObterUltimaCotacaoAsync(cust.Ticker);
            var precoAtual    = cotacao?.PrecoFechamento ?? 0m;
            var valorInv      = cust.Quantidade * cust.PrecoMedio;
            var valorAt       = cust.Quantidade * precoAtual;
            var plReais       = valorAt - valorInv;
            var plPercent     = valorInv > 0 ? Math.Round(plReais / valorInv * 100m, 2) : 0m;

            totInv   += valorInv;
            totAtual += valorAt;

            ativos.Add(new AtivoRentabilidadeResponse(
                cust.Ticker, cust.Quantidade, Math.Round(cust.PrecoMedio, 4),
                precoAtual, Math.Round(valorInv, 2), Math.Round(valorAt, 2),
                Math.Round(plReais, 2), plPercent, 0m));
        }

        // Calcula % de composição real por ativo
        var ativosComComp = ativos.Select(a => a with
        {
            ComposicaoCarteira = totAtual > 0
                ? Math.Round(a.ValorAtual / totAtual * 100m, 2)
                : 0m
        }).ToList();

        // ── 3. Comparação vs cesta recomendada ────────────────────────────────
        var comparacaoCesta = new List<ComparacaoCestaResponse>();

        if (cestaAtiva is not null)
        {
            foreach (var itemCesta in cestaAtiva.Itens)
            {
                var composicaoReal = ativosComComp
                    .FirstOrDefault(a => a.Ticker.TrimEnd('F') == itemCesta.Ticker)
                    ?.ComposicaoCarteira ?? 0m;

                comparacaoCesta.Add(new ComparacaoCestaResponse(
                    itemCesta.Ticker,
                    itemCesta.Percentual,
                    composicaoReal,
                    Math.Round(composicaoReal - itemCesta.Percentual, 2)));
            }
        }

        // ── 4. Histórico mensal de distribuições ──────────────────────────────
        // Agrupa por "YYYY-MM" usando a data de distribuição
        var historico = distribuicoes
            .GroupBy(d => d.DataDistribuicao.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new HistoricoMensalResponse(
                g.Key,
                g.Sum(d => d.Quantidade),
                Math.Round(g.Sum(d => d.ValorOperacao), 2),
                Math.Round(g.Sum(d => d.ValorIrDedoDuro), 2)))
            .ToList();

        // ── 5. IR dedo-duro acumulado total ──────────────────────────────────
        var irAcumulado = Math.Round(distribuicoes.Sum(d => d.ValorIrDedoDuro), 2);

        // ── 6. Métricas gerais ────────────────────────────────────────────────
        var plTotal  = Math.Round(totAtual - totInv, 2);
        var rentPerc = totInv > 0 ? Math.Round(plTotal / totInv * 100m, 2) : 0m;

        return new RentabilidadeResponse(
            cliente.Id, cliente.Nome, cliente.Ativo,
            Math.Round(totInv, 2), Math.Round(totAtual, 2),
            plTotal, rentPerc,
            ativosComComp,
            comparacaoCesta,
            historico,
            irAcumulado);
    }
}
