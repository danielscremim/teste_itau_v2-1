using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class RebalanceamentoRepository : IRebalanceamentoRepository
{
    private readonly AppDbContext _context;

    public RebalanceamentoRepository(AppDbContext context) => _context = context;

    public async Task<Rebalanceamento?> ObterPorIdAsync(long id)
        => await _context.Rebalanceamentos
            .Include(r => r.Clientes)
            .Include(r => r.CestaNova).ThenInclude(c => c!.Itens)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task AdicionarAsync(Rebalanceamento rebalanceamento)
        => await _context.Rebalanceamentos.AddAsync(rebalanceamento);

    public async Task<IEnumerable<Rebalanceamento>> ListarPendentesAsync()
        => await _context.Rebalanceamentos
            .Include(r => r.Clientes)
            .Include(r => r.CestaNova).ThenInclude(c => c!.Itens)
            .Where(r => r.Status == Domain.Enums.StatusRebalanceamento.Pendente)
            .ToListAsync();

    public void Atualizar(Rebalanceamento rebalanceamento)
        => _context.Rebalanceamentos.Update(rebalanceamento);

    public void AtualizarCliente(RebalanceamentoCliente item)
        => _context.RebalanceamentoClientes.Update(item);
}
