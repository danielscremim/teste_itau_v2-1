using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.UseCases.Admin;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ComprasProgramadas.API.Controllers;

/// <summary>
/// Endpoints de administração (cesta, cotações, motor de compra).
///
/// São endpoints que só o operador/gestor usa, não o cliente comum.
/// Em produção, seriam protegidos por autenticação/autorização (ex: JWT).
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly CadastrarCestaTopFiveUseCase  _cadastrarCesta;
    private readonly ObterCestaAtivaUseCase          _obterCestaAtiva;
    private readonly ListarHistoricoCestasUseCase    _listarHistorico;
    private readonly ImportarCotacoesUseCase         _importarCotacoes;
    private readonly ExecutarMotorCompraUseCase      _motorCompra;
    private readonly ExecutarRebalanceamentoUseCase  _rebalanceamento;

    private readonly IValidator<CadastrarCestaRequest>   _cestaValidator;
    private readonly IValidator<ImportarCotacoesRequest> _cotacoesValidator;

    public AdminController(
        CadastrarCestaTopFiveUseCase  cadastrarCesta,
        ObterCestaAtivaUseCase        obterCestaAtiva,
        ListarHistoricoCestasUseCase  listarHistorico,
        ImportarCotacoesUseCase       importarCotacoes,
        ExecutarMotorCompraUseCase    motorCompra,
        ExecutarRebalanceamentoUseCase rebalanceamento,
        IValidator<CadastrarCestaRequest>    cestaValidator,
        IValidator<ImportarCotacoesRequest>  cotacoesValidator)
    {
        _cadastrarCesta    = cadastrarCesta;
        _obterCestaAtiva   = obterCestaAtiva;
        _listarHistorico   = listarHistorico;
        _importarCotacoes  = importarCotacoes;
        _motorCompra       = motorCompra;
        _rebalanceamento   = rebalanceamento;
        _cestaValidator    = cestaValidator;
        _cotacoesValidator = cotacoesValidator;
    }

    /// <summary>
    /// POST /api/admin/cesta — Cadastra uma nova cesta Top Five (desativa a atual).
    /// </summary>
    [HttpPost("cesta")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarCesta([FromBody] CadastrarCestaRequest request)
    {
        var validacao = await _cestaValidator.ValidateAsync(request);
        if (!validacao.IsValid)
            return UnprocessableEntity(validacao.Errors.Select(e => new { campo = e.PropertyName, msg = e.ErrorMessage }));

        var resultado = await _cadastrarCesta.ExecutarAsync(request);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    /// <summary>
    /// GET /api/admin/cesta — Retorna a cesta Top Five atualmente ativa.
    /// </summary>
    [HttpGet("cesta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCestaAtiva()
    {
        var resultado = await _obterCestaAtiva.ExecutarAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// GET /api/admin/cesta/historico — Retorna todas as cestas já cadastradas (mais recente primeiro).
    /// </summary>
    [HttpGet("cesta/historico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarHistoricoCestas()
    {
        var resultado = await _listarHistorico.ExecutarAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// POST /api/admin/cotacoes/importar — Importa arquivo COTAHIST da B3.
    /// </summary>
    [HttpPost("cotacoes/importar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarCotacoes([FromBody] ImportarCotacoesRequest request)
    {
        var validacao = await _cotacoesValidator.ValidateAsync(request);
        if (!validacao.IsValid)
            return UnprocessableEntity(validacao.Errors.Select(e => new { campo = e.PropertyName, msg = e.ErrorMessage }));

        var resultado = await _importarCotacoes.ExecutarAsync(request);
        return Ok(resultado);
    }

    /// <summary>
    /// POST /api/admin/motor-compra — Executa o motor de compra consolidado.
    /// DataReferencia é opcional — se não informada, usa o dia de hoje.
    /// </summary>
    [HttpPost("motor-compra")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecutarMotorCompra([FromBody] ExecutarMotorCompraRequest? request)
    {
        var resultado = await _motorCompra.ExecutarAsync(request ?? new ExecutarMotorCompraRequest());
        return Ok(resultado);
    }

    /// <summary>
    /// POST /api/admin/rebalanceamento — Executa todos os rebalanceamentos pendentes.
    /// Disparado automaticamente ao cadastrar nova cesta, mas pode ser executado manualmente.
    /// </summary>
    [HttpPost("rebalanceamento")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecutarRebalanceamento()
    {
        var resultado = await _rebalanceamento.ExecutarAsync();
        return Ok(resultado);
    }
}
