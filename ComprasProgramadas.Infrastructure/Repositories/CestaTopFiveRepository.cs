using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class CestaTopFiveRepository : ICestaTopFiveRepository
{
    private readonly AppDbContext _context;

    public CestaTopFiveRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CestaTopFive?> ObterAtivaAsync()
        => await _context.CestasTopFive
            .Include(c => c.Itens) // traz os 5 ativos junto
            .FirstOrDefaultAsync(c => c.Ativa);

    public async Task<IEnumerable<CestaTopFive>> ListarHistoricoAsync()
        => await _context.CestasTopFive
            .Include(c => c.Itens)
            .OrderByDescending(c => c.DataAtivacao)
            .ToListAsync();

    public async Task AdicionarAsync(CestaTopFive cesta)
        => await _context.CestasTopFive.AddAsync(cesta);

    public void Atualizar(CestaTopFive cesta)
        => _context.CestasTopFive.Update(cesta);
}
