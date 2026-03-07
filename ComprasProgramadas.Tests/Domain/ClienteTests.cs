using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Domain;

/// <summary>
/// Testes da entidade Cliente.
///
/// O que é um teste unitário?
/// É como uma prova de matemática: você dá a entrada, faz a operação e confere se o resultado é o esperado.
///
/// Estrutura de cada teste (padrão AAA):
///   Arrange  = montar o cenário (criar objetos)
///   Act      = executar a ação que estamos testando
///   Assert   = verificar se o resultado foi o esperado
///
/// [Fact] = é um teste que sempre passou ou sempre falha (não depende de dados externos).
/// </summary>
public class ClienteTests
{
    // ──────────────────────────────────────────
    // TESTES DO MÉTODO Criar()
    // ──────────────────────────────────────────

    [Fact(DisplayName = "Criar cliente com dados válidos deve retornar cliente ativo")]
    public void Criar_DadosValidos_RetornaClienteAtivo()
    {
        // Arrange
        var nome       = "João da Silva";
        var cpf        = "12345678901";
        var email      = "joao@example.com";
        var valorMensal = 500m;

        // Act
        var cliente = Cliente.Criar(nome, cpf, email, valorMensal);

        // Assert
        // .Should().Be() é do FluentAssertions — lê-se "cliente.Nome deveria ser 'João da Silva'"
        cliente.Nome.Should().Be(nome);
        cliente.Cpf.Should().Be(cpf);
        cliente.Email.Should().Be(email);
        cliente.ValorMensal.Should().Be(valorMensal);
        cliente.Ativo.Should().BeTrue();
        cliente.DataAdesao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory(DisplayName = "Criar cliente com valor mensal menor que 100 deve lançar exceção")]
    [InlineData(0)]
    [InlineData(99.99)]
    [InlineData(-1)]
    // [Theory] + [InlineData] = o mesmo teste rodando com dados diferentes.
    // Pense como um teste de múltipla escolha: "para qualquer um desses valores, deve falhar"
    public void Criar_ValorMensalAbaixoDoMinimo_LancaDomainException(decimal valorInvalido)
    {
        // Act — o Action é uma "ação embrulhada" que ainda não foi executada
        Action act = () => Cliente.Criar("Nome", "12345678901", "email@x.com", valorInvalido);

        // Assert — Must.Should().Throw<>() verifica se a ação lança a exceção certa
        act.Should().Throw<DomainException>()
           .WithMessage("*100*"); // a mensagem deve conter "100"
    }

    // ──────────────────────────────────────────
    // TESTES DO MÉTODO Desativar()
    // ──────────────────────────────────────────

    [Fact(DisplayName = "Desativar cliente ativo deve setar Ativo = false e preencher DataSaida")]
    public void Desativar_ClienteAtivo_SetaInativo()
    {
        // Arrange
        var cliente = Cliente.Criar("Maria", "98765432100", "maria@x.com", 1000m);

        // Act
        cliente.Desativar();

        // Assert
        cliente.Ativo.Should().BeFalse();
        cliente.DataSaida.Should().NotBeNull();
        cliente.DataSaida!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Desativar cliente já inativo deve lançar DomainException")]
    public void Desativar_ClienteJaInativo_LancaDomainException()
    {
        // Arrange
        var cliente = Cliente.Criar("Pedro", "11122233344", "pedro@x.com", 300m);
        cliente.Desativar(); // primeira vez OK

        // Act — tenta desativar de novo (inválido)
        Action act = () => cliente.Desativar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ──────────────────────────────────────────
    // TESTES DO MÉTODO AlterarValorMensal()
    // ──────────────────────────────────────────

    [Fact(DisplayName = "AlterarValorMensal deve retornar o valor anterior e atualizar o valor atual")]
    public void AlterarValorMensal_NovoValorValido_RetornaValorAnteriorEAtualiza()
    {
        // Arrange
        var cliente  = Cliente.Criar("Ana", "55566677788", "ana@x.com", 500m);

        // Act
        var valorAnterior = cliente.AlterarValorMensal(750m);

        // Assert
        valorAnterior.Should().Be(500m);        // retornou o antigo
        cliente.ValorMensal.Should().Be(750m);  // e atualizou p/ o novo
    }

    [Fact(DisplayName = "AlterarValorMensal com valor abaixo de 100 deve lançar exceção")]
    public void AlterarValorMensal_ValorAbaixoDoMinimo_LancaDomainException()
    {
        // Arrange
        var cliente = Cliente.Criar("Carlos", "99988877766", "carlos@x.com", 300m);

        // Act
        Action act = () => cliente.AlterarValorMensal(50m);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*100*");
    }
}
