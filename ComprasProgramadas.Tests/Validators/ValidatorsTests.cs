using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.Validators;
using FluentAssertions;

namespace ComprasProgramadas.Tests.Validators;

/// <summary>
/// Testes dos Validators (FluentValidation).
///
/// O validator é como um porteiro: ele não deixa entrar uma requisição
/// com dados errados. Por exemplo, CPF com 10 dígitos é inválido —
/// o porteiro barra antes mesmo de chegar no banco de dados.
///
/// Como testar: chamamos validator.Validate(request)
/// e verificamos se o resultado tem erros ou não.
/// </summary>
public class ValidatorsTests
{
    // =====================================================================
    //  AdesaoValidator
    // =====================================================================

    [Fact(DisplayName = "AdesaoValidator deve passar com dados válidos")]
    public void AdesaoValidator_DadosValidos_Valida()
    {
        // Arrange
        var validator = new AdesaoValidator();
        var request   = new AdesaoRequest("João da Silva", "12345678901", "joao@email.com", 500m);

        // Act
        var resultado = validator.Validate(request);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "AdesaoValidator deve falhar com CPF com 10 dígitos")]
    public void AdesaoValidator_CpfCom10Digitos_Invalido()
    {
        var validator = new AdesaoValidator();
        var request   = new AdesaoRequest("João", "1234567890", "joao@email.com", 500m); // falta 1 dígito

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Cpf");
    }

    [Fact(DisplayName = "AdesaoValidator deve falhar com CPF com letras")]
    public void AdesaoValidator_CpfComLetras_Invalido()
    {
        var validator = new AdesaoValidator();
        var request   = new AdesaoRequest("João", "1234567890A", "joao@email.com", 500m);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Cpf");
    }

    [Fact(DisplayName = "AdesaoValidator deve falhar com e-mail inválido")]
    public void AdesaoValidator_EmailInvalido_Invalido()
    {
        var validator = new AdesaoValidator();
        var request   = new AdesaoRequest("João", "12345678901", "nao-e-email", 500m);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact(DisplayName = "AdesaoValidator deve falhar com nome vazio")]
    public void AdesaoValidator_NomeVazio_Invalido()
    {
        var validator = new AdesaoValidator();
        var request   = new AdesaoRequest("", "12345678901", "joao@email.com", 500m);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact(DisplayName = "AdesaoValidator deve falhar com valor mensal zero")]
    public void AdesaoValidator_ValorMensalZero_Invalido()
    {
        var validator = new AdesaoValidator();
        var request   = new AdesaoRequest("João", "12345678901", "joao@email.com", 0m);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ValorMensal");
    }

    // =====================================================================
    //  CadastrarCestaValidator
    // =====================================================================

    [Fact(DisplayName = "CadastrarCestaValidator deve passar com 5 itens que somam 100%")]
    public void CadastrarCestaValidator_CestaValida_Valida()
    {
        var validator = new CadastrarCestaValidator();
        var request   = new CadastrarCestaRequest(
        [
            new ItemCestaRequest("PETR4", 20m),
            new ItemCestaRequest("VALE3", 20m),
            new ItemCestaRequest("ITUB4", 20m),
            new ItemCestaRequest("B3SA3", 20m),
            new ItemCestaRequest("ABEV3", 20m)
        ]);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "CadastrarCestaValidator deve falhar com 4 itens")]
    public void CadastrarCestaValidator_4Itens_Invalido()
    {
        var validator = new CadastrarCestaValidator();
        var request   = new CadastrarCestaRequest(
        [
            new ItemCestaRequest("PETR4", 25m),
            new ItemCestaRequest("VALE3", 25m),
            new ItemCestaRequest("ITUB4", 25m),
            new ItemCestaRequest("B3SA3", 25m)
        ]);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "CadastrarCestaValidator deve falhar quando soma != 100%")]
    public void CadastrarCestaValidator_SomaDiferenteDe100_Invalido()
    {
        var validator = new CadastrarCestaValidator();
        var request   = new CadastrarCestaRequest(
        [
            new ItemCestaRequest("PETR4", 30m),
            new ItemCestaRequest("VALE3", 20m),
            new ItemCestaRequest("ITUB4", 20m),
            new ItemCestaRequest("B3SA3", 20m),
            new ItemCestaRequest("ABEV3", 5m)  // soma = 95, não 100
        ]);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "CadastrarCestaValidator deve falhar com percentual zero em um item")]
    public void CadastrarCestaValidator_PercentualZeroEmUmItem_Invalido()
    {
        var validator = new CadastrarCestaValidator();
        var request   = new CadastrarCestaRequest(
        [
            new ItemCestaRequest("PETR4", 100m),
            new ItemCestaRequest("VALE3", 0m),   // inválido
            new ItemCestaRequest("ITUB4", 0m),   // inválido
            new ItemCestaRequest("B3SA3", 0m),   // inválido
            new ItemCestaRequest("ABEV3", 0m)    // inválido
        ]);

        var resultado = validator.Validate(request);

        resultado.IsValid.Should().BeFalse();
    }

    // =====================================================================
    //  AlterarValorMensalValidator
    // =====================================================================

    [Fact(DisplayName = "AlterarValorMensalValidator deve passar com valor positivo")]
    public void AlterarValorMensalValidator_ValorPositivo_Valido()
    {
        var validator = new AlterarValorMensalValidator();
        var request   = new AlterarValorMensalRequest(500m);

        validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "AlterarValorMensalValidator deve falhar com valor zero ou negativo")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    public void AlterarValorMensalValidator_ValorZeroOuNegativo_Invalido(decimal valor)
    {
        var validator = new AlterarValorMensalValidator();
        var request   = new AlterarValorMensalRequest(valor);

        validator.Validate(request).IsValid.Should().BeFalse();
    }
}
