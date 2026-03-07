using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface ICestaTopFiveRepository
{
    Task<CestaTopFive?> ObterAtivaAsync();
    Task<IEnumerable<CestaTopFive>> ListarHistoricoAsync();
    Task AdicionarAsync(CestaTopFive cesta);
    void Atualizar(CestaTopFive cesta);
}
