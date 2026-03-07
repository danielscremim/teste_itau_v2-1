using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.UseCases.Clientes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ComprasProgramadas.API.Controllers;

/// <summary>
/// Endpoints do cliente (aderir, sair, alterar valor, consultar carteira, rentabilidade).
///
/// Como funciona um Controller no .NET?
/// Pense que ele é o "garçom" do restaurante:
///   - Recebe o pedido (request HTTP)
///   - Valida se o pedido faz sentido (validators)
///   - Passa para a cozinha (use case)
///   - Traz o prato para a mesa (response HTTP)
/// </summary>
[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly AderirAoProdutoUseCase        _aderir;
    private readonly SairDoProdutoUseCase          _sair;
    private readonly AlterarValorMensalUseCase     _alterarValor;
    private readonly ConsultarCarteiraUseCase      _consultarCarteira;
    private readonly ConsultarRentabilidadeUseCase _consultarRentabilidade;

    private readonly IValidator<AdesaoRequest>            _adesaoValidator;
    private readonly IValidator<AlterarValorMensalRequest> _alterarValidator;

    public ClienteController(
        AderirAoProdutoUseCase         aderir,
        SairDoProdutoUseCase           sair,
        AlterarValorMensalUseCase      alterarValor,
        ConsultarCarteiraUseCase       consultarCarteira,
        ConsultarRentabilidadeUseCase  consultarRentabilidade,
        IValidator<AdesaoRequest>             adesaoValidator,
        IValidator<AlterarValorMensalRequest> alterarValidator)
    {
        _aderir                 = aderir;
        _sair                   = sair;
        _alterarValor           = alterarValor;
        _consultarCarteira      = consultarCarteira;
        _consultarRentabilidade = consultarRentabilidade;
        _adesaoValidator        = adesaoValidator;
        _alterarValidator       = alterarValidator;
    }

    /// <summary>
    /// POST /api/clientes/adesao — Cadastra um novo cliente no produto.
    /// </summary>
    [HttpPost("adesao")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Aderir([FromBody] AdesaoRequest request)
    {
        var validacao = await _adesaoValidator.ValidateAsync(request);
        if (!validacao.IsValid)
            return UnprocessableEntity(validacao.Errors.Select(e => new { campo = e.PropertyName, msg = e.ErrorMessage }));

        var resultado = await _aderir.ExecutarAsync(request);
        return CreatedAtAction(nameof(ConsultarCarteira), new { clienteId = resultado.ClienteId }, resultado);
    }

    /// <summary>
    /// DELETE /api/clientes/{clienteId}/saida — Cancela a adesão do cliente.
    /// </summary>
    [HttpDelete("{clienteId:long}/saida")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sair(long clienteId)
    {
        var resultado = await _sair.ExecutarAsync(clienteId);
        return Ok(resultado);
    }

    /// <summary>
    /// PATCH /api/clientes/{clienteId}/valor-mensal — Altera o valor do aporte mensal.
    /// </summary>
    [HttpPatch("{clienteId:long}/valor-mensal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarValorMensal(long clienteId, [FromBody] AlterarValorMensalRequest request)
    {
        var validacao = await _alterarValidator.ValidateAsync(request);
        if (!validacao.IsValid)
            return UnprocessableEntity(validacao.Errors.Select(e => new { campo = e.PropertyName, msg = e.ErrorMessage }));

        var resultado = await _alterarValor.ExecutarAsync(clienteId, request);
        return Ok(resultado);
    }

    /// <summary>
    /// GET /api/clientes/{clienteId}/carteira — Retorna posição atual com P/L e rentabilidade.
    /// </summary>
    [HttpGet("{clienteId:long}/carteira")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarCarteira(long clienteId)
    {
        var resultado = await _consultarCarteira.ExecutarAsync(clienteId);
        return Ok(resultado);
    }

    /// <summary>
    /// GET /api/clientes/{clienteId}/rentabilidade — Visão detalhada de rentabilidade.
    ///
    /// Retorna além da carteira básica:
    ///   - P/L percentual por ativo
    ///   - Comparação da composição real vs cesta Top Five recomendada
    ///   - Histórico mensal de distribuições (evolução do patrimônio)
    ///   - IR dedo-duro acumulado retido
    ///
    /// Requisito obrigatório 4.1: "Acompanhamento de rentabilidade"
    /// </summary>
    [HttpGet("{clienteId:long}/rentabilidade")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarRentabilidade(long clienteId)
    {
        var resultado = await _consultarRentabilidade.ExecutarAsync(clienteId);
        return Ok(resultado);
    }
}
