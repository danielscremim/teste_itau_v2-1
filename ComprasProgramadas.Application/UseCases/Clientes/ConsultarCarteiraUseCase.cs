using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Clientes;

public class ConsultarCarteiraUseCase
{
    private readonly IClienteRepository          _clienteRepo;
    private readonly ICustodiaFilhoteRepository  _custodiaRepo;
    private readonly ICotacaoHistoricaRepository _cotacaoRepo;

    public ConsultarCarteiraUseCase(
        IClienteRepository clienteRepo,
        ICustodiaFilhoteRepository custodiaRepo,
        ICotacaoHistoricaRepository cotacaoRepo)
    { _clienteRepo = clienteRepo; _custodiaRepo = custodiaRepo; _cotacaoRepo = cotacaoRepo; }

    public async Task<CarteiraResponse> ExecutarAsync(long clienteId)
    {
        var cliente = await _clienteRepo.ObterPorIdAsync(clienteId)
            ?? throw new DomainException($"Cliente {clienteId} nao encontrado.");

        var custodias = await _custodiaRepo.ListarPorClienteAsync(clienteId);

        var ativos = new List<AtivoCustodiaResponse>();
        decimal totalInvestido = 0m, totalAtual = 0m;

        foreach (var cust in custodias.Where(c => c.Quantidade > 0))
        {
            var cotacao = await _cotacaoRepo.ObterUltimaCotacaoAsync(cust.Ticker);
            var cotacaoAtual   = cotacao?.PrecoFechamento ?? 0m;
            var valorInvestido = cust.Quantidade * cust.PrecoMedio;
            var valorAtual     = cust.Quantidade * cotacaoAtual;

            totalInvestido += valorInvestido;
            totalAtual     += valorAtual;

            ativos.Add(new AtivoCustodiaResponse(
                cust.Ticker, cust.Quantidade, cust.PrecoMedio,
                cotacaoAtual, valorAtual, valorAtual - valorInvestido, 0m));
        }

        var ativosComComp = ativos.Select(a => a with
        {
            ComposicaoPercent = totalAtual > 0 ? Math.Round(a.ValorAtual / totalAtual * 100m, 2) : 0m
        }).ToList();

        var plTotal = totalAtual - totalInvestido;
        var rent    = totalInvestido > 0 ? Math.Round(plTotal / totalInvestido * 100m, 2) : 0m;

        return new CarteiraResponse(
            cliente.Id, cliente.Nome,
            Math.Round(totalInvestido, 2), Math.Round(totalAtual, 2),
            Math.Round(plTotal, 2), rent,
            ativosComComp);
    }
}
