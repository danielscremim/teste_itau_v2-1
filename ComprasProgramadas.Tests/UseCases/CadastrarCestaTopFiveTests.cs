using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.UseCases.Admin;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ComprasProgramadas.Tests.UseCases;

/// <summary>
/// Testes do CadastrarCestaTopFiveUseCase.
///
/// Quando o administrador cadastra uma nova cesta:
///   1. A cesta antiga é desativada (se existir);
///   2. A nova cesta é criada e salva;
///   3. Para cada cliente ativo, cria-se um Rebalanceamento.
/// </summary>
public class CadastrarCestaTopFiveTests
{
    private readonly Mock<ICestaTopFiveRepository>    _cestaRepoMock    = new();
    private readonly Mock<IClienteRepository>         _clienteRepoMock  = new();
    private readonly Mock<IRebalanceamentoRepository> _rebalRepoMock    = new();
    private readonly Mock<IUnitOfWork>                _uowMock          = new();

    private CadastrarCestaTopFiveUseCase CriarUseCase() =>
        new(_cestaRepoMock.Object, _clienteRepoMock.Object, _rebalRepoMock.Object, _uowMock.Object);

    private static CadastrarCestaRequest CestaRequestValida() =>
        new(
        [
            new ItemCestaRequest("PETR4", 20m),
            new ItemCestaRequest("VALE3", 20m),
            new ItemCestaRequest("ITUB4", 20m),
            new ItemCestaRequest("B3SA3", 20m),
            new ItemCestaRequest("ABEV3", 20m)
        ], "admin.teste");

    [Fact(DisplayName = "ExecutarAsync sem cesta anterior deve criar nova cesta e retornar CestaResponse")]
    public async Task ExecutarAsync_SemCestaAnterior_CriaNovaCesta()
    {
        // Arrange
        _cestaRepoMock
            .Setup(r => r.ObterAtivaAsync())
            .ReturnsAsync((CestaTopFive?)null); // sem cesta ativa anterior

        _clienteRepoMock
            .Setup(r => r.ListarAtivosAsync())
            .ReturnsAsync([]); // sem clientes ativos (sem rebalanceamento)

        _uowMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(CestaRequestValida());

        // Assert
        resultado.Should().NotBeNull();
        resultado.Ativa.Should().BeTrue();
        resultado.Itens.Should().HaveCount(5);
        resultado.CriadoPor.Should().Be("admin.teste");

        // Confirma que AdicionarAsync foi chamado para criar a cesta
        _cestaRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<CestaTopFive>()), Times.Once);
    }

    [Fact(DisplayName = "ExecutarAsync com cesta anterior deve desativar a cesta antiga")]
    public async Task ExecutarAsync_ComCestaAnterior_DesativaCestaAntiga()
    {
        // Arrange
        var cestaAnterior = CestaTopFive.Criar(
        [
            ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("B3SA3", 20m), ("ABEV3", 20m)
        ], "admin.anterior");

        _cestaRepoMock
            .Setup(r => r.ObterAtivaAsync())
            .ReturnsAsync(cestaAnterior); // EXISTE uma cesta ativa

        _clienteRepoMock
            .Setup(r => r.ListarAtivosAsync())
            .ReturnsAsync([]); // sem clientes

        _uowMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var useCase = CriarUseCase();

        // Act
        await useCase.ExecutarAsync(CestaRequestValida());

        // Assert: Atualizar deve ter sido chamado para persistir a desativação
        _cestaRepoMock.Verify(r => r.Atualizar(cestaAnterior), Times.Once);
        cestaAnterior.Ativa.Should().BeFalse(); // a cesta antiga foi desativada
    }

    [Fact(DisplayName = "ExecutarAsync com clientes ativos deve criar Rebalanceamento")]
    public async Task ExecutarAsync_ComClientesAtivos_CriaRebalanceamento()
    {
        // Arrange
        _cestaRepoMock
            .Setup(r => r.ObterAtivaAsync())
            .ReturnsAsync((CestaTopFive?)null);

        var cliente = Cliente.Criar("Ana", "12345678901", "ana@email.com", 500m);
        _clienteRepoMock
            .Setup(r => r.ListarAtivosAsync())
            .ReturnsAsync([cliente]);

        _uowMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var useCase = CriarUseCase();

        // Act
        await useCase.ExecutarAsync(CestaRequestValida());

        // Assert: deve ter criado um Rebalanceamento para o cliente
        _rebalRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Rebalanceamento>()), Times.Once);
    }
}
