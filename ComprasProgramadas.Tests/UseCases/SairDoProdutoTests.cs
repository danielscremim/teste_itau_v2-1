using ComprasProgramadas.Application.UseCases.Clientes;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ComprasProgramadas.Tests.UseCases;

/// <summary>
/// Testes do SairDoProdutoUseCase.
/// O cliente pode pedir pra sair do produto (cancelar adesão).
/// O sistema marca o cliente como inativo (soft delete — não deleta do banco).
/// </summary>
public class SairDoProdutoTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IUnitOfWork>        _uowMock         = new();

    private SairDoProdutoUseCase CriarUseCase() =>
        new(_clienteRepoMock.Object, _uowMock.Object);

    [Fact(DisplayName = "ExecutarAsync com cliente ativo deve desativar e retornar resposta")]
    public async Task ExecutarAsync_ClienteAtivo_DesativaERetornaResposta()
    {
        // Arrange
        var cliente = Cliente.Criar("João", "12345678901", "joao@email.com", 500m);
        // Simula que o cliente existe no banco com id 1
        _clienteRepoMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(cliente);

        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(clienteId: 1);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Ativo.Should().BeFalse();                    // foi desativado
        resultado.Mensagem.Should().Contain("cancelada");     // mensagem de saída

        // Verifica que Atualizar foi chamado no repositório
        _clienteRepoMock.Verify(r => r.Atualizar(cliente), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "ExecutarAsync com ID inexistente deve lançar DomainException")]
    public async Task ExecutarAsync_ClienteInexistente_LancaDomainException()
    {
        // Arrange
        // ObterPorIdAsync retorna null → cliente não existe
        _clienteRepoMock
            .Setup(r => r.ObterPorIdAsync(99))
            .ReturnsAsync((Cliente?)null!);

        var useCase = CriarUseCase();

        // Act
        Func<Task> act = () => useCase.ExecutarAsync(clienteId: 99);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*99*");
    }

    [Fact(DisplayName = "ExecutarAsync com cliente já inativo deve lançar DomainException")]
    public async Task ExecutarAsync_ClienteJaInativo_LancaDomainException()
    {
        // Arrange
        var cliente = Cliente.Criar("Maria", "98765432100", "maria@email.com", 300m);
        cliente.Desativar(); // já está inativo

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(2)).ReturnsAsync(cliente);

        var useCase = CriarUseCase();

        // Act — tenta sair de novo
        Func<Task> act = () => useCase.ExecutarAsync(clienteId: 2);

        // Assert — a entidade vai lançar Domain Exception
        await act.Should().ThrowAsync<DomainException>();
    }
}
