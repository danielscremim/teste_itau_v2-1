using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface IVendaMesRepository
{
    /// <summary>
    /// Retorna o total de vendas do cliente no mês para verificar
    /// se ultrapassou R$ 20.000 (RN-057/RN-058).
    /// </summary>
    Task<decimal> ObterTotalVendasMesAsync(long clienteId, string mesReferencia);
    Task<decimal> ObterLucroLiquidoMesAsync(long clienteId, string mesReferencia);
    Task AdicionarAsync(VendaMes venda);
}
