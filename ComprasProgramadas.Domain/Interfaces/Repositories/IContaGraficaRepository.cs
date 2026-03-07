using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface IContaGraficaRepository
{
    Task<ContaGrafica?> ObterPorClienteAsync(long clienteId);
    Task<int> ContarFilhotesAsync();
    Task AdicionarAsync(ContaGrafica conta);
}
