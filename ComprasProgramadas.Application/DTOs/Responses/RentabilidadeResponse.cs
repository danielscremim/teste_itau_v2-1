namespace ComprasProgramadas.Application.DTOs.Responses;

/// <summary>
/// Resposta detalhada do endpoint GET /api/clientes/{id}/rentabilidade.
/// Inclui histórico de evolução e composição vs cesta recomendada (RN-010 + req. 4.1).
/// </summary>
public record RentabilidadeResponse(
    long   ClienteId,
    string NomeCliente,
    bool   Ativo,

    // ── Resumo financeiro ──────────────────────────────────────────────
    decimal TotalInvestido,
    decimal TotalAtual,
    decimal PlTotal,
    decimal RentabilidadePercent,

    // ── Detalhes por ativo ─────────────────────────────────────────────
    List<AtivoRentabilidadeResponse> Ativos,

    // ── Composição vs cesta recomendada ───────────────────────────────
    List<ComparacaoCestaResponse> ComparacaoCesta,

    // ── Histórico mensal de aportes/distribuições ─────────────────────
    List<HistoricoMensalResponse> HistoricoMensal,

    // ── IR estimado ────────────────────────────────────────────────────
    decimal IrDedoDuroAcumulado
);

/// <summary>
/// Detalhe de P/L por ativo com composição percentual na carteira.
/// </summary>
public record AtivoRentabilidadeResponse(
    string  Ticker,
    int     Quantidade,
    decimal PrecoMedio,
    decimal CotacaoAtual,
    decimal ValorInvestido,
    decimal ValorAtual,
    decimal PlReais,
    decimal PlPercent,
    decimal ComposicaoCarteira  // % do ativo no total da carteira
);

/// <summary>
/// Comparação de cada ativo: proporção atual na carteira vs proporção recomendada na cesta.
/// </summary>
public record ComparacaoCestaResponse(
    string  Ticker,
    decimal PercentualCesta,      // % recomendado na cesta Top Five
    decimal PercentualCarteira,   // % real na carteira do cliente
    decimal Desvio                // Carteira - Cesta (negativo = sub-alocado)
);

/// <summary>
/// Resumo mensal de distribuições recebidas (histórico de evolução da carteira).
/// </summary>
public record HistoricoMensalResponse(
    string  MesReferencia,       // "YYYY-MM"
    int     TotalAtivosComprados,
    decimal ValorTotalDistribuido,
    decimal IrDedoDuroMes
);
