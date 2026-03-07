namespace ComprasProgramadas.Application.DTOs.Responses;

/// <summary>
/// O que a API devolve após uma adesão bem-sucedida.
/// Baseado exatamente no contrato do arquivo exemplos-contratos-api.md
/// </summary>
public record AdesaoResponse(
    long             ClienteId,
    string           Nome,
    string           Cpf,
    string           Email,
    decimal          ValorMensal,
    bool             Ativo,
    DateTime         DataAdesao,
    ContaGraficaResponse ContaGrafica
);

public record ContaGraficaResponse(
    long     Id,
    string   NumeroConta,
    string   Tipo,
    DateTime DataCriacao
);

public record SaidaResponse(
    long     ClienteId,
    string   Nome,
    bool     Ativo,
    DateTime DataSaida,
    string   Mensagem
);

public record AlterarValorMensalResponse(
    long    ClienteId,
    string  Nome,
    decimal ValorAnterior,
    decimal ValorAtual,
    DateTime DataAlteracao
);
