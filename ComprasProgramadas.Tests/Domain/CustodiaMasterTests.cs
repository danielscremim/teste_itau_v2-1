using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da CustodiaMaster — o "cofre central" da corretora.
///
/// A conta Master guarda o RESÍDUO de ações que sobram após a distribuição.
/// Ex: sistema comprou 30 ações de PETR4, distribuiu 29 para os clientes.
///     → 1 ação sobra na Master para usar no próximo mês.
/// </summary>
public class CustodiaMasterTests
{
    [Fact(DisplayName = "AdicionarResiduo em custódia zerada deve definir PM igual ao preço")]
    public void AdicionarResiduo_CustodiaZerada_PMIgualAoPreco()
    {
        // Arrange
        var master = CustodiaMaster.Criar("PETR4");

        // Act
        master.AdicionarResiduo(3, 36m);

        // Assert
        master.Quantidade.Should().Be(3);
        master.PrecoMedio.Should().Be(36m);
    }

    [Fact(DisplayName = "AdicionarResiduo segunda vez deve recalcular PM ponderado")]
    public void AdicionarResiduo_DuasVezes_RecalculaPMPonderado()
    {
        // Arrange
        var master = CustodiaMaster.Criar("VALE3");
        master.AdicionarResiduo(5, 100m);  // 5 ações a R$100

        // Act
        master.AdicionarResiduo(5, 120m);  // mais 5 ações a R$120

        // Assert
        // PM = (5×100 + 5×120) / 10 = 1100 / 10 = R$110
        master.Quantidade.Should().Be(10);
        master.PrecoMedio.Should().Be(110m);
    }

    [Fact(DisplayName = "AdicionarResiduo com quantidade zero deve ser ignorado")]
    public void AdicionarResiduo_QuantidadeZero_NaoAlteraSaldo()
    {
        var master = CustodiaMaster.Criar("ITUB4");
        master.AdicionarResiduo(3, 50m);

        master.AdicionarResiduo(0, 60m); // deve ser ignorado

        master.Quantidade.Should().Be(3);
        master.PrecoMedio.Should().Be(50m);
    }

    [Fact(DisplayName = "Descontar quantidade válida deve reduzir saldo")]
    public void Descontar_QuantidadeValida_ReducSaldo()
    {
        // Arrange
        var master = CustodiaMaster.Criar("BBAS3");
        master.AdicionarResiduo(10, 30m);

        // Act
        master.Descontar(4);

        // Assert
        master.Quantidade.Should().Be(6);
    }

    [Fact(DisplayName = "Descontar mais do que o saldo disponível deve lançar DomainException")]
    public void Descontar_SaldoInsuficiente_LancaExcecao()
    {
        // Arrange
        var master = CustodiaMaster.Criar("WEGE3");
        master.AdicionarResiduo(3, 80m);

        // Act — tenta descontar 10 mas só tem 3
        Action act = () => master.Descontar(10);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
