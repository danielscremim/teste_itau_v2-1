using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Admin;

/// <summary>
/// Consulta a cesta Top Five que está ativa no momento.
///
/// Pense como um cardápio de restaurante:
///   este Use Case retorna o "cardápio do dia" — os 5 ativos
///   que o sistema vai comprar no próximo ciclo.
/// </summary>
public class ObterCestaAtivaUseCase
{
    private readonly ICestaTopFiveRepository _cestaRepo;

    public ObterCestaAtivaUseCase(ICestaTopFiveRepository cestaRepo)
    {
        _cestaRepo = cestaRepo;
    }

    public async Task<CestaResponse> ExecutarAsync()
    {
        var cesta = await _cestaRepo.ObterAtivaAsync()
            ?? throw new DomainException("Nenhuma cesta Top Five ativa encontrada.");

        return new CestaResponse(
            Id:              cesta.Id,
            Ativa:           cesta.Ativa,
            DataAtivacao:    cesta.DataAtivacao,
            DataDesativacao: cesta.DataDesativacao,
            CriadoPor:       cesta.CriadoPor,
            Itens:           cesta.Itens
                                 .Select(i => new ItemCestaResponse(i.Ticker, i.Percentual))
                                 .ToList()
        );
    }
}
