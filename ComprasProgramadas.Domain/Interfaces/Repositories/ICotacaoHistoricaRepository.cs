using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface ICotacaoHistoricaRepository
{
    /// <summary>
    /// Retorna a cotação de fechamento do último pregão disponível para um ticker.
    /// RN-027: sempre usar o pregão mais recente dos arquivos COTAHIST importados.
    /// </summary>
    Task<CotacaoHistorica?> ObterUltimaCotacaoAsync(string ticker);

    /// <summary>
    /// Busca a cotação de um ticker em uma data específica de pregão.
    /// Usado pelo motor de compra para pegar o fechamento de D-1.
    /// </summary>
    Task<CotacaoHistorica?> ObterPorTickerEDataAsync(string ticker, DateOnly dataPregao);

    Task<IEnumerable<string>> ListarArquivosImportadosAsync();
    Task AdicionarRangeAsync(IEnumerable<CotacaoHistorica> cotacoes);
}
