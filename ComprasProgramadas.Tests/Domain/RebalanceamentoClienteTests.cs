using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes do RebalanceamentoCliente — rastreia o progresso do rebalanceamento
/// para cada cliente individualmente.
/// </summary>
public class RebalanceamentoClienteTests
{
    [Fact(DisplayName = "Criar deve inicializar com status Pendente e valores zerados")]
    public void Criar_DadosValidos_StatusPendenteEValoresZerados()
    {
        // Act
        var item = RebalanceamentoCliente.Criar(rebalanceamentoId: 1, clienteId: 42);

        // Assert
        item.RebalanceamentoId.Should().Be(1);
        item.ClienteId.Should().Be(42);
        item.Status.Should().Be(StatusRebalanceamento.Pendente);
        item.TotalVendas.Should().Be(0m);
        item.TotalCompras.Should().Be(0m);
        item.IrDevido.Should().Be(0m);
        item.KafkaIrPublicado.Should().BeFalse();
        item.DataExecucao.Should().BeNull();
    }

    [Fact(DisplayName = "RegistrarResultado deve atualizar valores e marcar como Executado")]
    public void RegistrarResultado_AtualizaValoresEStatus()
    {
        // Arrange
        var item = RebalanceamentoCliente.Criar(1, 1);

        // Act
        item.RegistrarResultado(totalVendas: 5000m, totalCompras: 4000m, irDevido: 200m);

        // Assert
        item.TotalVendas.Should().Be(5000m);
        item.TotalCompras.Should().Be(4000m);
        item.IrDevido.Should().Be(200m);
        item.Status.Should().Be(StatusRebalanceamento.Executado);
        item.DataExecucao.Should().NotBeNull();
    }

    [Fact(DisplayName = "MarcarKafkaPublicado deve setar KafkaIrPublicado para true")]
    public void MarcarKafkaPublicado_SetaTrue()
    {
        var item = RebalanceamentoCliente.Criar(1, 1);

        item.MarcarKafkaPublicado();

        item.KafkaIrPublicado.Should().BeTrue();
    }

    [Fact(DisplayName = "MarcarErro deve mudar status para Erro")]
    public void MarcarErro_StatusMudaParaErro()
    {
        var item = RebalanceamentoCliente.Criar(1, 1);

        item.MarcarErro();

        item.Status.Should().Be(StatusRebalanceamento.Erro);
    }
}
