using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes do Rebalanceamento — processo de ajustar a carteira dos clientes
/// quando a cesta muda ou quando os percentuais ficam muito fora do alvo.
/// </summary>
public class RebalanceamentoTests
{
    [Fact(DisplayName = "CriarPorMudancaCesta deve definir tipo MudancaCesta e status Pendente")]
    public void CriarPorMudancaCesta_TipoEStatusCorretos()
    {
        // Act
        var rebal = Rebalanceamento.CriarPorMudancaCesta(cestaNovaId: 5);

        // Assert
        rebal.Tipo.Should().Be(TipoRebalanceamento.MudancaCesta);
        rebal.Status.Should().Be(StatusRebalanceamento.Pendente);
        rebal.CestaNovaId.Should().Be(5);
        rebal.DataFim.Should().BeNull(); // ainda não executado
    }

    [Fact(DisplayName = "CriarPorDesvio deve definir tipo Desvio e status Pendente")]
    public void CriarPorDesvio_TipoEStatusCorretos()
    {
        // Act
        var rebal = Rebalanceamento.CriarPorDesvio();

        // Assert
        rebal.Tipo.Should().Be(TipoRebalanceamento.DesvioProporcao);
        rebal.Status.Should().Be(StatusRebalanceamento.Pendente);
        rebal.CestaNovaId.Should().BeNull(); // Desvio não tem nova cesta
    }

    [Fact(DisplayName = "MarcarExecutado deve mudar status e registrar DataFim")]
    public void MarcarExecutado_StatusExecutadoEDataFimPreenchida()
    {
        var rebal = Rebalanceamento.CriarPorMudancaCesta(1);
        var antes  = DateTime.UtcNow;

        rebal.MarcarExecutado();

        rebal.Status.Should().Be(StatusRebalanceamento.Executado);
        rebal.DataFim.Should().NotBeNull().And.BeOnOrAfter(antes);
    }

    [Fact(DisplayName = "MarcarErro deve mudar status e registrar DataFim")]
    public void MarcarErro_StatusErroEDataFimPreenchida()
    {
        var rebal = Rebalanceamento.CriarPorDesvio();

        rebal.MarcarErro();

        rebal.Status.Should().Be(StatusRebalanceamento.Erro);
        rebal.DataFim.Should().NotBeNull();
    }
}
