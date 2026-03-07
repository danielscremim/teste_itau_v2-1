using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface IRebalanceamentoRepository
{
    Task<Rebalanceamento?> ObterPorIdAsync(long id);
    /// <summary>
    /// Lista todos os rebalanceamentos com status Pendente (aguardando execução).
    /// </summary>
    Task<IEnumerable<Rebalanceamento>> ListarPendentesAsync();
    Task AdicionarAsync(Rebalanceamento rebalanceamento);
    void Atualizar(Rebalanceamento rebalanceamento);
    void AtualizarCliente(RebalanceamentoCliente item);
}
