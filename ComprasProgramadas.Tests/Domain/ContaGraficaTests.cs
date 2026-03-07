using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da ContaGrafica — cada cliente tem uma conta "Filhote".
/// A corretora tem uma conta "Master" global.
///
/// Analogia: O cliente tem uma poupança individual (Filhote).
///            A corretora tem o cofre central (Master).
/// </summary>
public class ContaGraficaTests
{
    [Fact(DisplayName = "CriarFilhote deve associar o clienteId e definir o número da conta")]
    public void CriarFilhote_DadosValidos_ContaFilhoteAssociada()
    {
        // Act
        var conta = ContaGrafica.CriarFilhote(clienteId: 42, numeroConta: "FLH-000042");

        // Assert
        conta.ClienteId.Should().Be(42);
        conta.NumeroConta.Should().Be("FLH-000042");
        conta.Tipo.Should().Be(TipoConta.Filhote);
        conta.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "CriarMaster deve criar conta sem clienteId e com tipo Master")]
    public void CriarMaster_ContaMasterSemCliente()
    {
        // Act
        var master = ContaGrafica.CriarMaster();

        // Assert
        master.ClienteId.Should().BeNull();          // Master não pertence a nenhum cliente
        master.NumeroConta.Should().StartWith("MST"); // número da conta master começa com MST
        master.Tipo.Should().Be(TipoConta.Master);
    }

    [Fact(DisplayName = "CriarFilhote deve inicializar coleção de custódias vazia")]
    public void CriarFilhote_CustodiasInicialmenteVazias()
    {
        var conta = ContaGrafica.CriarFilhote(1, "FLH-000001");

        conta.CustodiasFilhote.Should().NotBeNull().And.BeEmpty();
    }
}
