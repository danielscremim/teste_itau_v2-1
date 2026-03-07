using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.UseCases.Admin;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ComprasProgramadas.Tests.UseCases;

/// <summary>
/// Testes do ExecutarMotorCompraUseCase — os caminhos de erro (validações).
///
/// O motor é muito complexo para testar toda a lógica aqui,
/// mas testamos as validações de entrada (pré-condições que causam erro).
/// </summary>
public class ExecutarMotorCompraTests
{
    private readonly Mock<IClienteRepository>          _clienteRepoMock  = new();
    private readonly Mock<ICestaTopFiveRepository>     _cestaRepoMock    = new();
    private readonly Mock<ICotacaoHistoricaRepository> _cotacaoRepoMock  = new();
    private readonly Mock<ICustodiaMasterRepository>   _masterRepoMock   = new();
    private readonly Mock<ICustodiaFilhoteRepository>  _filhoteRepoMock  = new();
    private readonly Mock<IContaGraficaRepository>     _contaRepoMock    = new();
    private readonly Mock<IOrdemCompraRepository>      _ordemRepoMock    = new();
    private readonly Mock<IDistribuicaoRepository>     _distribuicaoMock = new();
    private readonly Mock<IKafkaPublisher>             _kafkaMock        = new();
    private readonly Mock<IUnitOfWork>                 _uowMock          = new();

    private ExecutarMotorCompraUseCase CriarUseCase() =>
        new(_clienteRepoMock.Object, _cestaRepoMock.Object,
            _cotacaoRepoMock.Object, _masterRepoMock.Object,
            _filhoteRepoMock.Object, _contaRepoMock.Object,
            _ordemRepoMock.Object, _distribuicaoMock.Object,
            _kafkaMock.Object, _uowMock.Object);

    [Fact(DisplayName = "ExecutarAsync sem cesta ativa deve lançar DomainException")]
    public async Task ExecutarAsync_SemCestaAtiva_LancaDomainException()
    {
        // Arrange
        _cestaRepoMock
            .Setup(r => r.ObterAtivaAsync())
            .ReturnsAsync((CestaTopFive?)null); // sem cesta ativa

        var useCase = CriarUseCase();
        var request = new ExecutarMotorCompraRequest(null);

        // Act
        Func<Task> act = () => useCase.ExecutarAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cesta Top Five*");
    }

    [Fact(DisplayName = "ExecutarAsync sem clientes ativos deve lançar DomainException")]
    public async Task ExecutarAsync_SemClientesAtivos_LancaDomainException()
    {
        // Arrange: cesta existe, mas não há clientes ativos
        var cesta = CestaTopFive.Criar(
        [
            ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("B3SA3", 20m), ("ABEV3", 20m)
        ], "admin");

        _cestaRepoMock
            .Setup(r => r.ObterAtivaAsync())
            .ReturnsAsync(cesta);

        _clienteRepoMock
            .Setup(r => r.ListarAtivosAsync())
            .ReturnsAsync([]); // sem clientes

        var useCase = CriarUseCase();
        var request = new ExecutarMotorCompraRequest(null);

        // Act
        Func<Task> act = () => useCase.ExecutarAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cliente*");
    }
}
