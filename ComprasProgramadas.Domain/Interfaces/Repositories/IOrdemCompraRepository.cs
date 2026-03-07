using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface IOrdemCompraRepository
{
    Task<OrdemCompra?> ObterPorIdAsync(long id);
    /// <summary>
    /// Verifica se já existe uma OrdemCompra (executada ou pendente) para a data de referência.
    /// Usado pelo scheduler para evitar execução duplicada no mesmo dia.
    /// </summary>
    Task<bool> ExisteOrdemParaDataAsync(DateOnly dataReferencia);
    Task AdicionarAsync(OrdemCompra ordem);
    void Atualizar(OrdemCompra ordem);
}
