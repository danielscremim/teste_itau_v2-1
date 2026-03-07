using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface ICustodiaMasterRepository
{
    Task<CustodiaMaster?> ObterPorTickerAsync(string ticker);
    Task<IEnumerable<CustodiaMaster>> ListarTodosAsync();
    Task AdicionarAsync(CustodiaMaster custodia);
    void Atualizar(CustodiaMaster custodia);
}
