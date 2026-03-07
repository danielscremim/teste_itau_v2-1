namespace ComprasProgramadas.Application.DTOs.Responses;

public record CestaResponse(
    long              Id,
    bool              Ativa,
    DateTime          DataAtivacao,
    DateTime?         DataDesativacao,
    string?           CriadoPor,
    List<ItemCestaResponse> Itens
);

public record ItemCestaResponse(
    string  Ticker,
    decimal Percentual
);

public record MotorCompraResponse(
    long     OrdemId,
    DateOnly DataReferencia,
    decimal  TotalConsolidado,
    int      TotalClientesAtivos,
    List<ItemMotorResponse> Itens,
    string   Status
);

public record ItemMotorResponse(
    string  Ticker,
    decimal ValorAlvo,
    decimal CotacaoFechamento,
    int     QuantidadeCalculada,
    int     SaldoMasterDescontado,
    int     QuantidadeComprada,
    int     QtdLotePadrao,
    int     QtdFracionario
);

public record ImportacaoCotacoesResponse(
    string NomeArquivo,
    int    TotalRegistros,
    string Status
);

/// <summary>
/// Resposta do endpoint de execução de rebalanceamentos.
/// </summary>
public record RebalanceamentoResponse(
    int    TotalRebalanceamentos,
    int    TotalClientes,
    string Mensagem
);
