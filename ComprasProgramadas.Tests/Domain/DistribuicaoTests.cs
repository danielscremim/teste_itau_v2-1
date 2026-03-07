using ComprasProgramadas.Domain.Entities;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da Distribuicao — registra que um cliente recebeu ações.
///
/// Aqui também calculamos o IR dedo-duro (RN-053):
///   Alíquota = 0,005% = 0,00005 sobre o valor da operação.
///
/// Exemplo: comprou R$1.000 de ações
///   IR dedo-duro = R$1.000 × 0,00005 = R$0,05
///
/// É um imposto retido na fonte, automático.
/// </summary>
public class DistribuicaoTests
{
    [Fact(DisplayName = "Criar deve calcular ValorOperacao como quantidade × preço")]
    public void Criar_DeveCalcularValorOperacao()
    {
        // Arrange
        int quantidade       = 10;
        decimal precoUnit    = 36.50m;
        decimal valorEsperado = 10 * 36.50m; // R$365,00

        // Act
        var distribuicao = Distribuicao.Criar(
            ordemId: 1, clienteId: 1, ticker: "PETR4",
            quantidade: quantidade, precoUnitario: precoUnit,
            proporcaoCliente: 0.20m);

        // Assert
        distribuicao.ValorOperacao.Should().Be(valorEsperado);
    }

    [Fact(DisplayName = "Criar deve calcular IR dedo-duro como 0,005% do valor da operação (RN-053)")]
    public void Criar_DeveCalcularIrDedoDuro()
    {
        // Arrange: 100 ações × R$200 = R$20.000
        // IR = R$20.000 × 0,00005 = R$1,00
        var distribuicao = Distribuicao.Criar(
            ordemId: 1, clienteId: 1, ticker: "VALE3",
            quantidade: 100, precoUnitario: 200m,
            proporcaoCliente: 0.30m);

        // Assert
        distribuicao.ValorIrDedoDuro.Should().Be(1.00m); // 20000 × 0,00005 = 1,00
    }

    [Fact(DisplayName = "Criar com valor operação pequeno deve calcular IR com precisão de 2 casas")]
    public void Criar_ValorPequeno_IrArredondadoA2Casas()
    {
        // 3 ações × R$10 = R$30
        // IR = R$30 × 0,00005 = R$0,0015 → arredondado para R$0,00
        var distribuicao = Distribuicao.Criar(
            ordemId: 1, clienteId: 2, ticker: "BBAS3",
            quantidade: 3, precoUnitario: 10m,
            proporcaoCliente: 0.10m);

        // R$0,0015 arredondado a 2 casas = R$0,00
        distribuicao.ValorIrDedoDuro.Should().Be(0.00m);
    }

    [Fact(DisplayName = "Criar deve definir KafkaPublicado como false inicialmente")]
    public void Criar_KafkaPublicadoInicialmenteFalse()
    {
        var distribuicao = Distribuicao.Criar(1, 1, "ITUB4", 10, 50m, 0.25m);

        distribuicao.KafkaPublicado.Should().BeFalse();
    }

    [Fact(DisplayName = "MarcarKafkaPublicado deve setar KafkaPublicado para true")]
    public void MarcarKafkaPublicado_SetaTrue()
    {
        var distribuicao = Distribuicao.Criar(1, 1, "WEGE3", 5, 150m, 0.20m);

        distribuicao.MarcarKafkaPublicado();

        distribuicao.KafkaPublicado.Should().BeTrue();
    }

    [Fact(DisplayName = "Criar deve normalizar ticker para maiúsculas")]
    public void Criar_TickerMinusculo_ConverteMaiusculo()
    {
        var distribuicao = Distribuicao.Criar(1, 1, "abev3", 10, 15m, 0.20m);

        distribuicao.Ticker.Should().Be("ABEV3");
    }
}
