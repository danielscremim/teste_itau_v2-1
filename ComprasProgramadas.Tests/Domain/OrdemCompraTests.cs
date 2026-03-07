using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da OrdemCompra — representa uma execução do motor de compra.
/// É o "recibo" de todas as compras feitas em determinado ciclo.
/// </summary>
public class OrdemCompraTests
{
    [Fact(DisplayName = "Criar deve definir status Pendente e DataExecucao como UTC agora")]
    public void Criar_DadosValidos_StatusPendenteEDataPreenchida()
    {
        // Act
        var antes = DateTime.UtcNow;
        var ordem = OrdemCompra.Criar(cestaId: 1, DateOnly.FromDateTime(DateTime.Today), totalConsolidado: 5000m, arquivoCotacao: "COTAHIST_D01012024.TXT");
        var depois = DateTime.UtcNow;

        // Assert
        ordem.CestaId.Should().Be(1);
        ordem.TotalConsolidado.Should().Be(5000m);
        ordem.Status.Should().Be(StatusOrdem.Pendente);
        ordem.DataExecucao.Should().BeOnOrAfter(antes).And.BeOnOrBefore(depois);
    }

    [Fact(DisplayName = "MarcarExecutada deve alterar status para Executada")]
    public void MarcarExecutada_StatusMudaParaExecutada()
    {
        var ordem = OrdemCompra.Criar(1, DateOnly.FromDateTime(DateTime.Today), 1000m, "COTAHIST_D01012024.TXT");

        ordem.MarcarExecutada();

        ordem.Status.Should().Be(StatusOrdem.Executada);
    }

    [Fact(DisplayName = "MarcarErro deve alterar status para Erro")]
    public void MarcarErro_StatusMudaParaErro()
    {
        var ordem = OrdemCompra.Criar(1, DateOnly.FromDateTime(DateTime.Today), 1000m, null);

        ordem.MarcarErro();

        ordem.Status.Should().Be(StatusOrdem.Erro);
    }

    [Fact(DisplayName = "Criar com arquivoCotacao null deve funcionar")]
    public void Criar_SemArquivoOrigem_FuncionaNormalmente()
    {
        var ordem = OrdemCompra.Criar(2, DateOnly.FromDateTime(DateTime.Today), 0m, null);

        ordem.ArquivoCotacao.Should().BeNull();
        ordem.Itens.Should().BeEmpty();
    }

    [Fact(DisplayName = "Criar deve inicializar coleção de Itens vazia")]
    public void Criar_ColecaoItensInicialmenteVazia()
    {
        var ordem = OrdemCompra.Criar(1, DateOnly.FromDateTime(DateTime.Today), 5000m, "COTAHIST_D01012024.TXT");

        ordem.Itens.Should().NotBeNull().And.BeEmpty();
    }
}
