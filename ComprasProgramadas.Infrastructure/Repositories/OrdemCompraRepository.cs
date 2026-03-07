using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class OrdemCompraRepository : IOrdemCompraRepository
{
    private readonly AppDbContext _context;

    public OrdemCompraRepository(AppDbContext context) => _context = context;

    public async Task<OrdemCompra?> ObterPorIdAsync(long id)
        => await _context.OrdensCompra
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task AdicionarAsync(OrdemCompra ordem)
        => await _context.OrdensCompra.AddAsync(ordem);

    public async Task<bool> ExisteOrdemParaDataAsync(DateOnly dataReferencia)
        => await _context.OrdensCompra
            .AnyAsync(o => o.DataReferencia == dataReferencia);

    public void Atualizar(OrdemCompra ordem)
        => _context.OrdensCompra.Update(ordem);
}
