using ComprasProgramadas.Domain.Entities;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes de ItemOrdemCompra — o cálculo de quanto comprar de cada ação.
///
/// Imagine que o sistema vai comprar PETR4 para vários clientes.
/// A cotação está em R$35,00 e um cliente tem R$1.000 mensais → quantas ações?
///
///   TRUNCAR(1000 / 35) = TRUNCAR(28,57) = 28 ações
///
/// Note: não arredonda! 28,99 vira 28, nunca 29.
///
/// Depois, as ações são separadas em:
///   - Lote padrão: múltiplos de 100 → se comprou 153, lote = 100
///   - Fracionário: sobra → 53 ações, com ticker + "F" (ex: PETR4F)
/// </summary>
public class ItemOrdemCompraTests
{
    [Fact(DisplayName = "Calcular deve TRUNCAR (nunca arredondar) a quantidade")]
    public void Calcular_SempreAplicaTruncar()
    {
        // Arrange
        // 1000 / 36.15 = 27.66... → TRUNCAR = 27 (não 28)
        decimal valorAlvo = 1_000m;
        decimal cotacao   = 36.15m;

        // Act
        var item = ItemOrdemCompra.Calcular(ordemId: 1, "PETR4", valorAlvo, cotacao, saldoMaster: 0);

        // Assert
        item.QuantidadeCalculada.Should().Be(27); // Truncate(27.66) = 27
    }

    [Fact(DisplayName = "Calcular com 153 ações deve separar lote=100 fracionário=53")]
    public void Calcular_153Acoes_SeparaLoteEFracionario()
    {
        // Arrange
        // Para chegar exatamente em 153: valorAlvo = 15300, cotacao = 100
        var item = ItemOrdemCompra.Calcular(1, "VALE3", valorAlvo: 15_300m, cotacaoFechamento: 100m, saldoMaster: 0);

        // Assert
        item.QuantidadeAComprar.Should().Be(153);
        item.QtdLotePadrao.Should().Be(100);
        item.QtdFracionario.Should().Be(53);
        item.TickerFracionario.Should().Be("VALE3F"); // ticker + "F"
    }

    [Fact(DisplayName = "Calcular com múltiplo exato de 100 não deve ter fracionário")]
    public void Calcular_MultiploExato_SemFracionario()
    {
        // 20000 / 100 = 200 → múltiplo exato, sem fracionário
        var item = ItemOrdemCompra.Calcular(1, "ITUB4", valorAlvo: 20_000m, cotacaoFechamento: 100m, saldoMaster: 0);

        item.QuantidadeAComprar.Should().Be(200);
        item.QtdLotePadrao.Should().Be(200);
        item.QtdFracionario.Should().Be(0);
        item.TickerFracionario.Should().BeNull(); // sem fracionário, null
    }

    [Fact(DisplayName = "Calcular deve descontar saldo da master antes de comprar")]
    public void Calcular_ComSaldoMaster_DescontaDoTotalAComprar()
    {
        // Arrange
        // valorAlvo = 1000, cotacao = 10 → calculada = 100
        // saldoMaster = 10 → o sistema usa 10 do estoque, compra só 90
        var item = ItemOrdemCompra.Calcular(1, "BBAS3", valorAlvo: 1_000m, cotacaoFechamento: 10m, saldoMaster: 10);

        // Assert
        item.QuantidadeCalculada.Should().Be(100);
        item.SaldoMasterDescontado.Should().Be(10);
        item.QuantidadeAComprar.Should().Be(90);
    }

    [Fact(DisplayName = "SaldoMaster maior que a quantidade calculada deve usar só o necessário")]
    public void Calcular_SaldoMasterSuficiente_NaoCompraNada()
    {
        // Arrange: calcula 5 ações, mas a master já tem 20 sobrando
        var item = ItemOrdemCompra.Calcular(1, "RENT3", valorAlvo: 500m, cotacaoFechamento: 100m, saldoMaster: 20);

        // Assert: usa apenas 5 da master (o que precisa), compra 0
        item.QuantidadeCalculada.Should().Be(5);
        item.SaldoMasterDescontado.Should().Be(5);  // não usa mais do que precisa
        item.QuantidadeAComprar.Should().Be(0);
    }

    [Fact(DisplayName = "QuantidadeTotalDisponivel deve somar compradas + saldo master usado")]
    public void QuantidadeTotalDisponivel_SomaCompradasMaisMaster()
    {
        // 300 ações calculadas, master descontou 50 → compra 250, total = 250 + 50 = 300
        var item = ItemOrdemCompra.Calcular(1, "ABEV3", valorAlvo: 30_000m, cotacaoFechamento: 100m, saldoMaster: 50);

        item.QuantidadeTotalDisponivel.Should().Be(300);
    }

    [Fact(DisplayName = "Calcular deve normalizar ticker para maiúsculas")]
    public void Calcular_TickerMinusculo_ConverteMaiusculo()
    {
        var item = ItemOrdemCompra.Calcular(1, "petr4", 1_000m, 100m, 0);

        item.Ticker.Should().Be("PETR4");
    }
}
