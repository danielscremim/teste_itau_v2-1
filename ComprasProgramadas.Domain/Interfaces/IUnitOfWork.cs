namespace ComprasProgramadas.Domain.Interfaces;

/// <summary>
/// Unit of Work: garante que múltiplas operações de banco sejam commitadas
/// em uma única transação atômica.
///
/// Por que isso importa?
/// O motor de compra faz várias escritas: salva OrdemCompra, múltiplas Distribuicoes,
/// atualiza CustodiaFilhote e registra Distribuicoes.
/// Se qualquer passo falhar, TODAS as escritas devem ser desfeitas juntas.
/// O UnitOfWork garante isso via transação de banco de dados.
///
/// Sem UnitOfWork, você poderia ter uma situação onde a OrdemCompra foi salva
/// mas as Distribuicoes não — o banco ficaria em estado inconsistente.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
