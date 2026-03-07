using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da VendaMes — registra quando um cliente vendeu ações em um determinado mês.
///
/// O lucro é calculado automaticamente:
///   Lucro = Quantidade × (Preço de Venda - Preço Médio)
///
/// Se vendeu mais caro do que comprou → lucro positivo
/// Se vendeu mais barato do que comprou → lucro negativo (prejuízo)
/// </summary>
public class VendaMesTests
{
    [Fact(DisplayName = "Registrar deve calcular ValorTotalVenda como quantidade × precoVenda")]
    public void Registrar_DeveCalcularValorTotalVenda()
    {
        // Act: 5 ações vendidas a R$50 = R$250
        var venda = VendaMes.Registrar(1, "PETR4", 5, 50m, 40m, OrigemVenda.RebalanceamentoCesta);

        venda.ValorTotalVenda.Should().Be(250m); // 5 × R$50
    }

    [Fact(DisplayName = "Registrar deve calcular Lucro como quantidade × (precoVenda - PM)")]
    public void Registrar_DeveCalcularLucro()
    {
        // Comprou a R$40, vendeu a R$50 → lucro = 5 × R$10 = R$50
        var venda = VendaMes.Registrar(1, "VALE3", 5, 50m, 40m, OrigemVenda.RebalanceamentoCesta);

        venda.Lucro.Should().Be(50m);
    }

    [Fact(DisplayName = "Registrar com prejuízo deve ter lucro negativo")]
    public void Registrar_ComPrejuizo_LucroNegativo()
    {
        // Comprou a R$60, vendeu a R$50 → prejuízo = 3 × (-R$10) = -R$30
        var venda = VendaMes.Registrar(1, "ITUB4", 3, 50m, 60m, OrigemVenda.RebalanceamentoCesta);

        venda.Lucro.Should().Be(-30m);
    }

    [Fact(DisplayName = "Registrar deve normalizar ticker para maiúsculas")]
    public void Registrar_TickerMinusculo_ConverteMaiusculo()
    {
        var venda = VendaMes.Registrar(1, "abev3", 1, 20m, 15m, OrigemVenda.RebalanceamentoCesta);

        venda.Ticker.Should().Be("ABEV3");
    }

    [Fact(DisplayName = "Registrar deve definir MesReferencia no formato YYYY-MM")]
    public void Registrar_MesReferenciaFormatadoCorreto()
    {
        var esperado = DateTime.UtcNow.ToString("yyyy-MM");
        var venda    = VendaMes.Registrar(1, "WEGE3", 10, 150m, 120m, OrigemVenda.RebalanceamentoCesta);

        venda.MesReferencia.Should().Be(esperado);
    }
}
