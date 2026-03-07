using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    // O repositório recebe o DbContext "de fora" (injeção de dependência)
    // — assim ele usa o mesmo contexto que todos os outros repositórios num
    // mesmo request HTTP (garantindo que a transação seja compartilhada)
    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObterPorIdAsync(long id)
        => await _context.Clientes
            .Include(c => c.ContaGrafica) // Include = JOIN para trazer a conta junto
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Cliente?> ObterPorCpfAsync(string cpf)
        => await _context.Clientes
            .FirstOrDefaultAsync(c => c.Cpf == cpf);

    public async Task<IEnumerable<Cliente>> ListarAtivosAsync()
        => await _context.Clientes
            .Where(c => c.Ativo) // WHERE ativo = 1
            .ToListAsync();

    public async Task AdicionarAsync(Cliente cliente)
        => await _context.Clientes.AddAsync(cliente);

    public void Atualizar(Cliente cliente)
        => _context.Clientes.Update(cliente);
}
