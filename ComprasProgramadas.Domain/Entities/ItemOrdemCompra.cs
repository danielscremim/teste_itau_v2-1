namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Detalhe por ativo dentro de uma OrdemCompra.
/// Guarda os cálculos completos do motor para auditoria.
///
/// Campo por campo:
/// - ValorAlvo:              TotalConsolidado × %ativo (ex: R$3.500 × 30% = R$1.050)
/// - CotacaoFechamento:      preço do último pregão no COTAHIST (RN-027)
/// - QuantidadeCalculada:    TRUNCAR(ValorAlvo / Cotacao) (RN-028)
/// - SaldoMasterDescontado:  residuo anterior da CustodiaMaster (RN-030)
/// - QuantidadeAComprar:     QuantidadeCalculada - SaldoMasterDescontado
/// - QtdLotePadrao:          múltiplos de 100 (RN-031) — em lote = QTd / 100 × 100
/// - QtdFracionario:         restante 1-99 (RN-032) — ticker recebe sufixo F (RN-033)
/// </summary>
public class ItemOrdemCompra : EntidadeBase
{
    public long    OrdemId                  { get; private set; }
    public string  Ticker                   { get; private set; } = string.Empty;
    public decimal ValorAlvo                { get; private set; }
    public decimal CotacaoFechamento        { get; private set; }
    public int     QuantidadeCalculada      { get; private set; }
    public int     SaldoMasterDescontado    { get; private set; }
    public int     QuantidadeAComprar       { get; private set; }
    public int     QtdLotePadrao            { get; private set; }
    public int     QtdFracionario           { get; private set; }
    public string? TickerFracionario        { get; private set; } // ex: PETR4F

    // Navegação
    public OrdemCompra? Ordem { get; private set; }

    protected ItemOrdemCompra() { }

    public static ItemOrdemCompra Calcular(
        long    ordemId,
        string  ticker,
        decimal valorAlvo,
        decimal cotacaoFechamento,
        int     saldoMaster)
    {
        // RN-028: TRUNCAR(ValorAlvo / Cotacao)
        var qtdCalculada = (int)Math.Truncate(valorAlvo / cotacaoFechamento);

        // RN-030: descontar saldo da master (nunca pode ficar negativo)
        var saldoDesconto = Math.Min(saldoMaster, qtdCalculada);
        var qtdAComprar   = qtdCalculada - saldoDesconto;

        // RN-031/RN-032: separar lote padrão do fracionário
        var qtdLote       = (qtdAComprar / 100) * 100; // múltiplos de 100
        var qtdFracionario = qtdAComprar % 100;          // restante

        return new ItemOrdemCompra
        {
            OrdemId               = ordemId,
            Ticker                = ticker.ToUpper(),
            ValorAlvo             = valorAlvo,
            CotacaoFechamento     = cotacaoFechamento,
            QuantidadeCalculada   = qtdCalculada,
            SaldoMasterDescontado = saldoDesconto,
            QuantidadeAComprar    = qtdAComprar,
            QtdLotePadrao         = qtdLote,
            QtdFracionario        = qtdFracionario,
            TickerFracionario     = qtdFracionario > 0 ? ticker.ToUpper() + "F" : null // RN-033
        };
    }

    /// <summary>
    /// Quantidade total disponível para distribuição = compradas + saldo master usado.
    /// RN-037: "quantidade total disponível = compradas + saldo master anterior"
    /// </summary>
    public int QuantidadeTotalDisponivel => QuantidadeAComprar + SaldoMasterDescontado;
}
