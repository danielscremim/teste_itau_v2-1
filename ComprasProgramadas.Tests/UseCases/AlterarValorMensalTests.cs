using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.UseCases.Clientes;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ComprasProgramadas.Tests.UseCases;

/// <summary>
/// Testes do AlterarValorMensalUseCase.
/// O cliente pode mudar quanto investe por mês.
/// O sistema guarda um histórico de todas as alterações.
/// </summary>
public class AlterarValorMensalTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IUnitOfWork>        _uowMock         = new();

    private AlterarValorMensalUseCase CriarUseCase() =>
        new(_clienteRepoMock.Object, _uowMock.Object);

    [Fact(DisplayName = "ExecutarAsync deve retornar valor anterior e novo valor")]
    public async Task ExecutarAsync_ClienteAtivo_RetornaValorAnteriorENovo()
    {
        // Arrange: cliente tem R$500 de valor mensal
        var cliente = Cliente.Criar("João", "12345678901", "joao@email.com", 500m);

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(cliente);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new AlterarValorMensalRequest(800m);
        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(clienteId: 1, request);

        // Assert
        resultado.ValorAnterior.Should().Be(500m);    // era 500
        resultado.ValorAtual.Should().Be(800m);       // agora é 800
        resultado.Nome.Should().Be("João");
    }

    [Fact(DisplayName = "ExecutarAsync deve adicionar HistoricoValorMensal à coleção do cliente")]
    public async Task ExecutarAsync_DeveCriarHistorico()
    {
        // Arrange
        var cliente = Cliente.Criar("Ana", "11122233344", "ana@email.com", 200m);

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(cliente);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var useCase = CriarUseCase();

        // Act
        await useCase.ExecutarAsync(1, new AlterarValorMensalRequest(500m));

        // Assert: histórico deve ter sido criado e adicionado ao cliente
        cliente.HistoricoValorMensal.Should().HaveCount(1);
        var historico = cliente.HistoricoValorMensal.First();
        historico.ValorAnterior.Should().Be(200m);
        historico.ValorNovo.Should().Be(500m);
    }

    [Fact(DisplayName = "ExecutarAsync com cliente inativo deve lançar DomainException")]
    public async Task ExecutarAsync_ClienteInativo_LancaDomainException()
    {
        // Arrange
        var cliente = Cliente.Criar("Pedro", "55566677788", "pedro@email.com", 300m);
        cliente.Desativar(); // cliente está inativo

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(cliente);

        var useCase = CriarUseCase();

        // Act
        Func<Task> act = () => useCase.ExecutarAsync(1, new AlterarValorMensalRequest(500m));

        // Assert
        await act.Should().ThrowAsync<DomainException>().WithMessage("*inativo*");
    }

    [Fact(DisplayName = "ExecutarAsync com cliente inexistente deve lançar DomainException")]
    public async Task ExecutarAsync_ClienteInexistente_LancaDomainException()
    {
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((Cliente?)null!);

        var useCase = CriarUseCase();

        Func<Task> act = () => useCase.ExecutarAsync(99, new AlterarValorMensalRequest(500m));

        await act.Should().ThrowAsync<DomainException>().WithMessage("*99*");
    }
}
