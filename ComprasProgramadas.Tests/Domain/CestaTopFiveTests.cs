using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da CestaTopFive — valida as regras RN-014 a RN-017.
/// </summary>
public class CestaTopFiveTests
{
    // ── helper: cria uma lista válida de 5 itens que somam 100% ──
    private static List<(string, decimal)> CestaValida() =>
    [
        ("PETR4", 20m),
        ("VALE3", 20m),
        ("ITUB4", 20m),
        ("B3SA3", 20m),
        ("ABEV3", 20m)
    ];

    [Fact(DisplayName = "Criar cesta com 5 itens somando 100% deve ter Ativa = true")]
    public void Criar_DadosValidos_RetornaCestaAtiva()
    {
        // Act
        var cesta = CestaTopFive.Criar(CestaValida(), "analista@itau.com");

        // Assert
        cesta.Ativa.Should().BeTrue();
        cesta.Itens.Should().HaveCount(5);
        cesta.CriadoPor.Should().Be("analista@itau.com");
        cesta.DataAtivacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Criar cesta com 4 itens deve lançar DomainException (RN-014)")]
    public void Criar_QuatroItens_LancaDomainException()
    {
        // Arrange — apenas 4 itens (inválido)
        var itens = new List<(string, decimal)>
        {
            ("PETR4", 25m), ("VALE3", 25m), ("ITUB4", 25m), ("B3SA3", 25m)
        };

        // Act
        Action act = () => CestaTopFive.Criar(itens);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*5*");
    }

    [Fact(DisplayName = "Criar cesta com soma diferente de 100% deve lançar DomainException (RN-015)")]
    public void Criar_SomaDiferenteDe100_LancaDomainException()
    {
        // Arrange — soma = 90% (inválido)
        var itens = new List<(string, decimal)>
        {
            ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("B3SA3", 20m), ("ABEV3", 10m) // 90%
        };

        Action act = () => CestaTopFive.Criar(itens);

        act.Should().Throw<DomainException>().WithMessage("*100*");
    }

    [Fact(DisplayName = "Criar cesta com percentual zero deve lançar DomainException (RN-016)")]
    public void Criar_PercentualZero_LancaDomainException()
    {
        // Arrange — um item com 0% (inválido — seria como reservar uma vaga para ninguém)
        var itens = new List<(string, decimal)>
        {
            ("PETR4", 0m), ("VALE3", 25m), ("ITUB4", 25m), ("B3SA3", 25m), ("ABEV3", 25m) // soma ok, mas 0% inválido
        };

        Action act = () => CestaTopFive.Criar(itens);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Desativar cesta ativa deve setar Ativa = false e preencher DataDesativacao")]
    public void Desativar_CestaAtiva_SetaInativa()
    {
        // Arrange
        var cesta = CestaTopFive.Criar(CestaValida());

        // Act
        cesta.Desativar();

        // Assert
        cesta.Ativa.Should().BeFalse();
        cesta.DataDesativacao.Should().NotBeNull();
    }

    [Fact(DisplayName = "Tickers dos itens devem ser convertidos para maiúsculas")]
    public void Criar_TickerMinusculo_ConverteMaiusculo()
    {
        // Arrange
        var itens = new List<(string, decimal)>
        {
            ("petr4", 20m), ("vale3", 20m), ("itub4", 20m), ("b3sa3", 20m), ("abev3", 20m)
        };

        // Act
        var cesta = CestaTopFive.Criar(itens);

        // Assert
        cesta.Itens.Select(i => i.Ticker).Should().AllSatisfy(t => t.Should().BeUpperCased());
    }
}
