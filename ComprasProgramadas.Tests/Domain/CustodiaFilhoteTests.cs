using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da CustodiaFilhote — a carteira individual de cada cliente.
///
/// A regra mais importante aqui é o Preço Médio (PM):
///   Quando você compra mais ações, o PM é recalculado:
///   PM_novo = (QtdAntiga × PM_antigo + QtdNova × PrecoNovo) / (QtdAntiga + QtdNova)
///
///   Exemplo:
///     Tinha 8 ações PM = R$35,00
///     Comprou 10 a R$37,00
///     Novo PM = (8×35 + 10×37) / 18 = (280 + 370) / 18 = R$36,11...
///
/// E na VENDA: o PM NÃO muda — só a quantidade (RN-043).
/// </summary>
public class CustodiaFilhoteTests
{
    [Fact(DisplayName = "RegistrarCompra na custódia zerada deve definir PM igual ao preço da compra")]
    public void RegistrarCompra_CustodiaZerada_PMIgualAoPreco()
    {
        // Arrange
        var custodia = CustodiaFilhote.Criar(clienteId: 1, contaGraficaId: 1, ticker: "PETR4");

        // Act
        custodia.RegistrarCompra(10, 30m);

        // Assert
        custodia.Quantidade.Should().Be(10);
        custodia.PrecoMedio.Should().Be(30m);
    }

    [Fact(DisplayName = "RegistrarCompra segunda vez deve recalcular PM ponderado (RN-042)")]
    public void RegistrarCompra_SegundaCompra_RecalculaPMPonderado()
    {
        // Arrange: 8 ações a R$35,00 (PM = 35)
        var custodia = CustodiaFilhote.Criar(1, 1, "VALE3");
        custodia.RegistrarCompra(8, 35m);

        // Act: compra mais 10 a R$37,00
        custodia.RegistrarCompra(10, 37m);

        // Assert
        // PM = (8×35 + 10×37) / 18 = 650 / 18 ≈ 36,1111...
        var pmEsperado = (8m * 35m + 10m * 37m) / 18m;
        custodia.Quantidade.Should().Be(18);
        custodia.PrecoMedio.Should().BeApproximately(pmEsperado, 0.001m);
    }

    [Fact(DisplayName = "RegistrarCompra com quantidade zero deve lançar DomainException")]
    public void RegistrarCompra_QuantidadeZero_LancaExcecao()
    {
        var custodia = CustodiaFilhote.Criar(1, 1, "ITUB4");

        Action act = () => custodia.RegistrarCompra(0, 30m);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "RegistrarVenda deve reduzir quantidade mas NÃO alterar PM (RN-043)")]
    public void RegistrarVenda_NaoAlteraPrecoMedio()
    {
        // Arrange: comprou 20 ações a R$40
        var custodia = CustodiaFilhote.Criar(1, 1, "BBAS3");
        custodia.RegistrarCompra(20, 40m);

        // Act: vende 5 por R$50
        var lucro = custodia.RegistrarVenda(5, 50m);

        // Assert
        custodia.Quantidade.Should().Be(15);       // 20 - 5
        custodia.PrecoMedio.Should().Be(40m);      // PM NÃO muda
        lucro.Should().Be(5 * (50m - 40m));        // lucro = 5 × R$10 = R$50
    }

    [Fact(DisplayName = "RegistrarVenda com lucro negativo (prejuízo) deve retornar valor negativo")]
    public void RegistrarVenda_ComPrejuizo_RetornaLucroNegativo()
    {
        // Arrange: comprou a R$50, vai vender a R$40 (prejuízo)
        var custodia = CustodiaFilhote.Criar(1, 1, "WEGE3");
        custodia.RegistrarCompra(10, 50m);

        // Act
        var lucro = custodia.RegistrarVenda(10, 40m);

        // Assert
        lucro.Should().BeNegative(); // prejuízo de R$100
        lucro.Should().Be(-100m);
    }

    [Fact(DisplayName = "RegistrarVenda com quantidade maior que disponível deve lançar DomainException")]
    public void RegistrarVenda_QuantidadeMaiorQueDisponivel_LancaExcecao()
    {
        var custodia = CustodiaFilhote.Criar(1, 1, "RENT3");
        custodia.RegistrarCompra(5, 100m);

        Action act = () => custodia.RegistrarVenda(10, 110m); // tem 5, tenta vender 10

        act.Should().Throw<DomainException>();
    }
}
