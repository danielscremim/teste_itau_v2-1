using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Admin;

/// <summary>
/// Retorna todas as cestas já cadastradas, da mais recente para a mais antiga.
///
/// É como um "histórico de cardápios" — mostra todas as cestas
/// que já foram usadas, incluindo as que foram substituídas.
/// </summary>
public class ListarHistoricoCestasUseCase
{
    private readonly ICestaTopFiveRepository _cestaRepo;

    public ListarHistoricoCestasUseCase(ICestaTopFiveRepository cestaRepo)
    {
        _cestaRepo = cestaRepo;
    }

    public async Task<IEnumerable<CestaResponse>> ExecutarAsync()
    {
        var cestas = await _cestaRepo.ListarHistoricoAsync();

        return cestas
            .OrderByDescending(c => c.DataAtivacao)
            .Select(c => new CestaResponse(
                Id:              c.Id,
                Ativa:           c.Ativa,
                DataAtivacao:    c.DataAtivacao,
                DataDesativacao: c.DataDesativacao,
                CriadoPor:       c.CriadoPor,
                Itens:           c.Itens
                                  .Select(i => new ItemCestaResponse(i.Ticker, i.Percentual))
                                  .ToList()
            ));
    }
}
