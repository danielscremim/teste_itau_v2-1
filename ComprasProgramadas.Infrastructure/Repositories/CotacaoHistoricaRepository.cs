using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Repositories;

public class CotacaoHistoricaRepository : ICotacaoHistoricaRepository
{
    private readonly AppDbContext _context;

    public CotacaoHistoricaRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// Busca a cotação de fechamento MAIS RECENTE de um ticker.
    /// RN-027: sempre usar o pregão mais recente disponível.
    ///
    /// Exemplo: se temos PETR4 nos dias 24, 25 e 26 de fevereiro,
    /// retorna a do dia 26 (OrderByDescending + FirstOrDefault).
    /// </summary>
    public async Task<CotacaoHistorica?> ObterUltimaCotacaoAsync(string ticker)
        => await _context.CotacoesHistoricas
            .Where(c => c.Ticker == ticker)
            .OrderByDescending(c => c.DataPregao)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Busca a cotação de um ticker em uma data específica de pregão.
    /// Usado pelo motor de compra para pegar o fechamento de D-1 (RN-025).
    /// </summary>
    public async Task<CotacaoHistorica?> ObterPorTickerEDataAsync(string ticker, DateOnly dataPregao)
        => await _context.CotacoesHistoricas
            .Where(c => c.Ticker == ticker && c.DataPregao == dataPregao)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<string>> ListarArquivosImportadosAsync()
        => await _context.CotacoesHistoricas
            .Select(c => c.ArquivoOrigem)
            .Distinct()
            .ToListAsync();

    public async Task AdicionarRangeAsync(IEnumerable<CotacaoHistorica> cotacoes)
        => await _context.CotacoesHistoricas.AddRangeAsync(cotacoes);
}
