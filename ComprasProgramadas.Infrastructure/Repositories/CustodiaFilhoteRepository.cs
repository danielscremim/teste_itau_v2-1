using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class CustodiaFilhoteRepository : ICustodiaFilhoteRepository
{
    private readonly AppDbContext _context;

    public CustodiaFilhoteRepository(AppDbContext context) => _context = context;

    public async Task<CustodiaFilhote?> ObterPorClienteETickerAsync(long clienteId, string ticker)
        => await _context.CustodiaFilhote
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Ticker == ticker);

    public async Task<IEnumerable<CustodiaFilhote>> ListarPorClienteAsync(long clienteId)
        => await _context.CustodiaFilhote
            .Where(c => c.ClienteId == clienteId && c.Quantidade > 0)
            .ToListAsync();

    public async Task AdicionarAsync(CustodiaFilhote custodia)
        => await _context.CustodiaFilhote.AddAsync(custodia);

    public void Atualizar(CustodiaFilhote custodia)
    {
        // Se a entidade foi recém criada nesta mesma transação (estado Added),
        // o EF já vai fazer INSERT no commit — não precisa chamar Update.
        // Chamar Update nesse caso causaria erro de "Id temporário".
        if (_context.Entry(custodia).State != Microsoft.EntityFrameworkCore.EntityState.Added)
            _context.CustodiaFilhote.Update(custodia);
    }
}
