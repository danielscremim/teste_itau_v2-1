using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class CustodiaMasterRepository : ICustodiaMasterRepository
{
    private readonly AppDbContext _context;

    public CustodiaMasterRepository(AppDbContext context) => _context = context;

    public async Task<CustodiaMaster?> ObterPorTickerAsync(string ticker)
        => await _context.CustodiaMaster
            .FirstOrDefaultAsync(c => c.Ticker == ticker);

    public async Task<IEnumerable<CustodiaMaster>> ListarTodosAsync()
        => await _context.CustodiaMaster.ToListAsync();

    public async Task AdicionarAsync(CustodiaMaster custodia)
        => await _context.CustodiaMaster.AddAsync(custodia);

    public void Atualizar(CustodiaMaster custodia)
    {
        if (_context.Entry(custodia).State != Microsoft.EntityFrameworkCore.EntityState.Added)
            _context.CustodiaMaster.Update(custodia);
    }
}
