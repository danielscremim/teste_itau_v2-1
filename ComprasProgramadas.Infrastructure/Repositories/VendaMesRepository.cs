using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class VendaMesRepository : IVendaMesRepository
{
    private readonly AppDbContext _context;

    public VendaMesRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// Soma todas as vendas do cliente no mês.
    /// Ex: busca todas as linhas onde ClienteId = 1 e MesReferencia = "2026-03"
    /// e soma a coluna ValorTotalVenda.
    /// RN-057: isso determina se o cliente ultrapassa R$ 20.000 no mês.
    /// </summary>
    public async Task<decimal> ObterTotalVendasMesAsync(long clienteId, string mesReferencia)
        => await _context.VendasMes
            .Where(v => v.ClienteId == clienteId && v.MesReferencia == mesReferencia)
            .SumAsync(v => v.ValorTotalVenda);

    public async Task<decimal> ObterLucroLiquidoMesAsync(long clienteId, string mesReferencia)
        => await _context.VendasMes
            .Where(v => v.ClienteId == clienteId && v.MesReferencia == mesReferencia)
            .SumAsync(v => v.Lucro);

    public async Task AdicionarAsync(VendaMes venda)
        => await _context.VendasMes.AddAsync(venda);
}
