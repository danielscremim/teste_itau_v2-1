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
/// Testes do AderirAoProdutoUseCase.
///
/// Como esse Use Case chama o banco de dados, usamos o MOCK (Moq).
/// Mock é como um dublê de cinema: parece um repositório real,
/// mas a gente controla exatamente o que ele retorna.
///
/// Assim testamos só a LÓGICA do Use Case sem precisar de banco real.
/// </summary>
public class AderirAoProdutoTests
{
    private readonly Mock<IClienteRepository>      _clienteRepoMock = new();
    private readonly Mock<IContaGraficaRepository> _contaRepoMock   = new();
    private readonly Mock<IUnitOfWork>             _uowMock         = new();

    private AderirAoProdutoUseCase CriarUseCase() =>
        new(_clienteRepoMock.Object, _contaRepoMock.Object, _uowMock.Object);

    [Fact(DisplayName = "ExecutarAsync com CPF novo deve criar cliente e conta gráfica")]
    public async Task ExecutarAsync_CpfNovo_CriaClienteERetornaAdesaoResponse()
    {
        // Arrange
        // ObterPorCpfAsync retorna null → CPF ainda não existe no sistema
        _clienteRepoMock
            .Setup(r => r.ObterPorCpfAsync("12345678901"))
            .ReturnsAsync((Cliente?)null);

        // ContarFilhotesAsync retorna 0 → será o primeiro cliente
        _contaRepoMock
            .Setup(r => r.ContarFilhotesAsync())
            .ReturnsAsync(0);

        // CommitAsync retorna int (afetados), usamos ReturnsAsync(1)
        _uowMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request    = new AdesaoRequest("João da Silva", "12345678901", "joao@email.com", 500m);
        var useCase    = CriarUseCase();

        // Act
        var resultado  = await useCase.ExecutarAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("João da Silva");
        resultado.Cpf.Should().Be("12345678901");
        resultado.ContaGrafica.NumeroConta.Should().Be("FLH-000001"); // 0+1 = 1
        resultado.Ativo.Should().BeTrue();
    }

    [Fact(DisplayName = "ExecutarAsync com CPF já cadastrado deve lançar DomainException")]
    public async Task ExecutarAsync_CpfDuplicado_LancaDomainException()
    {
        // Arrange
        // Simula que o CPF JÁ existe no banco
        var clienteExistente = Cliente.Criar("Maria", "12345678901", "maria@email.com", 300m);
        _clienteRepoMock
            .Setup(r => r.ObterPorCpfAsync("12345678901"))
            .ReturnsAsync(clienteExistente);

        var request = new AdesaoRequest("João", "12345678901", "joao@email.com", 500m);
        var useCase = CriarUseCase();

        // Act
        Func<Task> act = () => useCase.ExecutarAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*12345678901*");
    }

    [Fact(DisplayName = "ExecutarAsync deve gerar número de conta sequencial com padding de zeros")]
    public async Task ExecutarAsync_DecimoCliente_NumeroConta000010()
    {
        // Arrange
        _clienteRepoMock.Setup(r => r.ObterPorCpfAsync(It.IsAny<string>())).ReturnsAsync((Cliente?)null);
        _contaRepoMock.Setup(r => r.ContarFilhotesAsync()).ReturnsAsync(9); // já existem 9
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new AdesaoRequest("Carlos", "99988877766", "carlos@email.com", 1000m);
        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(request);

        // Assert
        resultado.ContaGrafica.NumeroConta.Should().Be("FLH-000010"); // 9+1 = 10
    }
}
