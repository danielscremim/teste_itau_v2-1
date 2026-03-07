using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class ContaGraficaRepository : IContaGraficaRepository
{
    private readonly AppDbContext _context;

    public ContaGraficaRepository(AppDbContext context) => _context = context;

    public async Task<ContaGrafica?> ObterPorClienteAsync(long clienteId)
        => await _context.ContasGraficas
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

    public async Task<int> ContarFilhotesAsync()
        => await _context.ContasGraficas
            .CountAsync(c => c.ClienteId != null); // conta apenas as filhotes

    public async Task AdicionarAsync(ContaGrafica conta)
        => await _context.ContasGraficas.AddAsync(conta);
}
