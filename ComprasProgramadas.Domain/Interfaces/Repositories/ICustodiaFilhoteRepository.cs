using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface ICustodiaFilhoteRepository
{
    Task<CustodiaFilhote?> ObterPorClienteETickerAsync(long clienteId, string ticker);
    Task<IEnumerable<CustodiaFilhote>> ListarPorClienteAsync(long clienteId);
    Task AdicionarAsync(CustodiaFilhote custodia);
    void Atualizar(CustodiaFilhote custodia);
}
