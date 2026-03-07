using FluentValidation;
using ComprasProgramadas.Application.DTOs.Requests;

namespace ComprasProgramadas.Application.Validators;

/// <summary>
/// Valida os dados de adesão de um novo cliente.
/// FluentValidation funciona assim: você declara regras em RuleFor(x => x.Campo)
/// e o framework dispara o erro automaticamente se a regra falhar.
/// </summary>
public class AdesaoValidator : AbstractValidator<AdesaoRequest>
{
    public AdesaoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Matches(@"^\d{11}$").WithMessage("CPF deve conter exatamente 11 dígitos numéricos (sem pontos ou traços).");
            // Nota: validação de dígito verificador pode ser adicionada aqui num projeto real.

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.ValorMensal)
            .GreaterThan(0).WithMessage("Valor mensal deve ser maior que zero.");
            // O desafio não cita mínimo explícito, mas o senso comum manda > 0.
    }
}

public class AlterarValorMensalValidator : AbstractValidator<AlterarValorMensalRequest>
{
    public AlterarValorMensalValidator()
    {
        RuleFor(x => x.NovoValorMensal)
            .GreaterThan(0).WithMessage("Novo valor mensal deve ser maior que zero.");
    }
}
