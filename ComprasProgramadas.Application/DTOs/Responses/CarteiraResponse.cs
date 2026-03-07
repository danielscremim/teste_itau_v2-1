namespace ComprasProgramadas.Application.DTOs.Responses;

/// <summary>
/// Tela de carteira do cliente — exibe posição atual com P/L e rentabilidade.
/// Baseado nas RN-063 a RN-070 e no exemplo da tabela de rentabilidade do documento.
/// </summary>
public record CarteiraResponse(
    long                     ClienteId,
    string                   Nome,
    decimal                  ValorInvestidoTotal,   // soma de (qtd × PM) de todos os ativos
    decimal                  ValorAtualTotal,        // soma de (qtd × cotação atual)
    decimal                  PlTotal,                // ValorAtual - ValorInvestido
    decimal                  RentabilidadePercent,   // (PlTotal / ValorInvestido) × 100
    List<AtivoCustodiaResponse> Ativos
);

public record AtivoCustodiaResponse(
    string  Ticker,
    int     Quantidade,
    decimal PrecoMedio,       // RN-067
    decimal CotacaoAtual,     // RN-069
    decimal ValorAtual,       // Quantidade × CotacaoAtual — RN-063
    decimal Pl,               // (CotacaoAtual - PrecoMedio) × Quantidade — RN-064
    decimal ComposicaoPercent // % que esse ativo representa no total — RN-070
);
