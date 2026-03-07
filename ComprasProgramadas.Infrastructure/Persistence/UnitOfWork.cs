using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Infrastructure.Data;

namespace ComprasProgramadas.Infrastructure.Persistence;

/// <summary>
/// O UnitOfWork é como apertar o botão "Salvar" do Word.
///
/// Sem UnitOfWork:
///   Repositório A salva → banco atualizado
///   Repositório B falha → banco fica na metade (inconsistente!)
///
/// Com UnitOfWork:
///   Repositório A prepara → nada no banco ainda
///   Repositório B prepara → nada no banco ainda
///   CommitAsync() → tudo vai de uma vez, ou não vai nada (transação atômica)
///
/// O EF Core já gerencia as transações por DbContext — ao chamar
/// SaveChangesAsync(), ele commita tudo que foi adicionado/atualizado.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
