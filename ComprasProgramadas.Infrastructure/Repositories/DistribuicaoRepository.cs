using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class DistribuicaoRepository : IDistribuicaoRepository
{
    private readonly AppDbContext _context;

    public DistribuicaoRepository(AppDbContext context) => _context = context;

    public async Task AdicionarAsync(Distribuicao distribuicao)
        => await _context.Distribuicoes.AddAsync(distribuicao);

    // Busca distribuições ainda não publicadas no Kafka — para reprocessamento em caso de falha
    public async Task<IEnumerable<Distribuicao>> ListarNaoPublicadasKafkaAsync()
        => await _context.Distribuicoes
            .Where(d => !d.KafkaPublicado)
            .ToListAsync();

    public async Task<IEnumerable<Distribuicao>> ListarPorClienteAsync(long clienteId)
        => await _context.Distribuicoes
            .Where(d => d.ClienteId == clienteId)
            .OrderBy(d => d.DataDistribuicao)
            .ToListAsync();

    public void Atualizar(Distribuicao distribuicao)
        => _context.Distribuicoes.Update(distribuicao);
}
