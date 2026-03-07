namespace ComprasProgramadas.Application.DTOs.Requests;

/// <summary>
/// Dados que o cliente envia ao aderir ao produto.
/// Corresponde ao contrato do POST /api/clientes/adesao
/// </summary>
public record AdesaoRequest(
    string  Nome,
    string  Cpf,
    string  Email,
    decimal ValorMensal
);

public record SaidaRequest(); // sem body — só o clienteId na URL

public record AlterarValorMensalRequest(decimal NovoValorMensal);
