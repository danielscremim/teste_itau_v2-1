using ComprasProgramadas.Application.UseCases.Clientes;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ComprasProgramadas.Tests.UseCases;

/// <summary>
/// Testes do ConsultarCarteiraUseCase.
///
/// Consultar a carteira mostra:
///   - Quais ações o cliente tem
///   - Preço Médio de cada uma
///   - Cotação atual (buscada do banco)
///   - P/L (lucro ou prejuízo) = valorAtual - valorInvestido
///   - Rentabilidade % = P/L / valorInvestido × 100
/// </summary>
public class ConsultarCarteiraTests
{
    private readonly Mock<IClienteRepository>          _clienteRepoMock = new();
    private readonly Mock<ICustodiaFilhoteRepository>  _custodiaRepoMock = new();
    private readonly Mock<ICotacaoHistoricaRepository> _cotacaoRepoMock = new();

    private ConsultarCarteiraUseCase CriarUseCase() =>
        new(_clienteRepoMock.Object, _custodiaRepoMock.Object, _cotacaoRepoMock.Object);

    [Fact(DisplayName = "ExecutarAsync deve calcular P/L positivo quando cotação sobe")]
    public async Task ExecutarAsync_CotacaoSobe_PLPositivo()
    {
        // Arrange
        var cliente = Cliente.Criar("João", "12345678901", "joao@email.com", 500m);

        // Custódia: 10 ações de PETR4 compradas a R$30 (PM = 30)
        var custodia = CustodiaFilhote.Criar(1, 1, "PETR4");
        custodia.RegistrarCompra(10, 30m);

        // Cotação atual: R$40 (subiu R$10 por ação)
        var cotacao = CotacaoHistorica.Criar("PETR4", DateOnly.FromDateTime(DateTime.Today),
            40m, 42m, 39m, 40m, "COTAHIST_D01012024.TXT");

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(cliente);
        _custodiaRepoMock.Setup(r => r.ListarPorClienteAsync(1)).ReturnsAsync([custodia]);
        _cotacaoRepoMock.Setup(r => r.ObterUltimaCotacaoAsync("PETR4")).ReturnsAsync(cotacao);

        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(1);

        // Assert
        resultado.ValorInvestidoTotal.Should().Be(300m);  // 10 × R$30
        resultado.ValorAtualTotal.Should().Be(400m);      // 10 × R$40
        resultado.PlTotal.Should().Be(100m);               // lucro R$100
        resultado.RentabilidadePercent.Should().BePositive();
    }

    [Fact(DisplayName = "ExecutarAsync deve calcular P/L negativo quando cotação cai")]
    public async Task ExecutarAsync_CotacaoCai_PLNegativo()
    {
        // Arrange
        var cliente  = Cliente.Criar("Maria", "98765432100", "maria@email.com", 1000m);
        var custodia = CustodiaFilhote.Criar(1, 1, "VALE3");
        custodia.RegistrarCompra(10, 50m); // comprou a R$50

        // Cotação caiu para R$45 (prejuízo de R$5 por ação)
        var cotacao = CotacaoHistorica.Criar("VALE3", DateOnly.FromDateTime(DateTime.Today),
            45m, 46m, 44m, 45m, "COTAHIST_D01012024.TXT");

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(cliente);
        _custodiaRepoMock.Setup(r => r.ListarPorClienteAsync(1)).ReturnsAsync([custodia]);
        _cotacaoRepoMock.Setup(r => r.ObterUltimaCotacaoAsync("VALE3")).ReturnsAsync(cotacao);

        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(1);

        // Assert
        resultado.ValorInvestidoTotal.Should().Be(500m);  // 10 × R$50
        resultado.ValorAtualTotal.Should().Be(450m);      // 10 × R$45
        resultado.PlTotal.Should().Be(-50m);               // prejuízo R$50
        resultado.RentabilidadePercent.Should().BeNegative();
    }

    [Fact(DisplayName = "ExecutarAsync sem custódias deve retornar carteira vazia com P/L zero")]
    public async Task ExecutarAsync_SemCustodias_CarteiraVazia()
    {
        // Arrange
        var cliente = Cliente.Criar("Carlos", "11122233344", "carlos@email.com", 300m);

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(cliente);
        _custodiaRepoMock.Setup(r => r.ListarPorClienteAsync(1)).ReturnsAsync([]); // sem ativos

        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(1);

        // Assert
        resultado.ValorInvestidoTotal.Should().Be(0m);
        resultado.ValorAtualTotal.Should().Be(0m);
        resultado.PlTotal.Should().Be(0m);
        resultado.RentabilidadePercent.Should().Be(0m);
        resultado.Ativos.Should().BeEmpty();
    }

    [Fact(DisplayName = "ExecutarAsync com cliente inexistente deve lançar DomainException")]
    public async Task ExecutarAsync_ClienteInexistente_LancaDomainException()
    {
        // Arrange
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((Cliente?)null!);

        var useCase = CriarUseCase();

        // Act
        Func<Task> act = () => useCase.ExecutarAsync(99);

        // Assert
        await act.Should().ThrowAsync<DomainException>().WithMessage("*99*");
    }

    [Fact(DisplayName = "ExecutarAsync deve calcular composição % de cada ativo na carteira")]
    public async Task ExecutarAsync_DoisAtivos_CalculaComposicaoPercent()
    {
        // Arrange: 2 ativos com valor atual igual → cada um = 50%
        var cliente = Cliente.Criar("Ana", "55566677788", "ana@email.com", 2000m);

        var custodiaA = CustodiaFilhote.Criar(1, 1, "PETR4");
        custodiaA.RegistrarCompra(10, 100m); // valor atual = 10 × R$100 = R$1.000

        var custodiaB = CustodiaFilhote.Criar(1, 1, "VALE3");
        custodiaB.RegistrarCompra(10, 100m); // valor atual = 10 × R$100 = R$1.000

        var cotacaoA = CotacaoHistorica.Criar("PETR4", DateOnly.FromDateTime(DateTime.Today),
            100m, 100m, 100m, 100m, "COTAHIST_D01012024.TXT");
        var cotacaoB = CotacaoHistorica.Criar("VALE3", DateOnly.FromDateTime(DateTime.Today),
            100m, 100m, 100m, 100m, "COTAHIST_D01012024.TXT");

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(cliente);
        _custodiaRepoMock.Setup(r => r.ListarPorClienteAsync(1)).ReturnsAsync([custodiaA, custodiaB]);
        _cotacaoRepoMock.Setup(r => r.ObterUltimaCotacaoAsync("PETR4")).ReturnsAsync(cotacaoA);
        _cotacaoRepoMock.Setup(r => r.ObterUltimaCotacaoAsync("VALE3")).ReturnsAsync(cotacaoB);

        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecutarAsync(1);

        // Assert
        resultado.ValorAtualTotal.Should().Be(2000m); // 1000 + 1000
        resultado.Ativos.Should().HaveCount(2);
        resultado.Ativos.Should().AllSatisfy(a => a.ComposicaoPercent.Should().Be(50m));
    }
}
