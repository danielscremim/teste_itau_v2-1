namespace ComprasProgramadas.Application.DTOs.Requests;

/// <summary>
/// Dados para criar ou alterar a cesta Top Five.
/// A lista deve conter exatamente 5 itens com percentuais somando 100%.
/// </summary>
public record CadastrarCestaRequest(
    List<ItemCestaRequest> Itens,
    string? CriadoPor = null
);

public record ItemCestaRequest(
    string  Ticker,
    decimal Percentual
);

public record ImportarCotacoesRequest(
    string NomeArquivo // ex: "COTAHIST_D20260225.TXT"
);

public record ExecutarMotorCompraRequest(
    DateOnly? DataReferencia = null // null = usa a data atual
);
