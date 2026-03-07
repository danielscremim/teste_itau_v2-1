using FluentValidation;
using ComprasProgramadas.Application.DTOs.Requests;

namespace ComprasProgramadas.Application.Validators;

/// <summary>
/// Valida o cadastro de uma nova cesta Top Five.
/// Regras de negócio validadas aqui (RN-059 a RN-061):
///   - Exatamente 5 tickers
///   - Cada percentual > 0%
///   - Soma dos percentuais = 100%
/// </summary>
public class CadastrarCestaValidator : AbstractValidator<CadastrarCestaRequest>
{
    public CadastrarCestaValidator()
    {
        RuleFor(x => x.Itens)
            .NotNull().WithMessage("A lista de itens não pode ser nula.")
            .Must(itens => itens.Count == 5)
                .WithMessage("A cesta deve conter exatamente 5 ativos.");

        RuleForEach(x => x.Itens).SetValidator(new ItemCestaValidator());

        RuleFor(x => x.Itens)
            .Must(itens => itens != null && Math.Abs(itens.Sum(i => i.Percentual) - 100m) < 0.0001m)
            .WithMessage("A soma dos percentuais dos 5 itens deve ser exatamente 100%.");
    }
}

public class ItemCestaValidator : AbstractValidator<ItemCestaRequest>
{
    public ItemCestaValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker é obrigatório.")
            .MaximumLength(10).WithMessage("Ticker deve ter no máximo 10 caracteres.");

        RuleFor(x => x.Percentual)
            .GreaterThan(0).WithMessage("Percentual de cada ativo deve ser maior que 0%.");
    }
}

public class ImportarCotacoesValidator : AbstractValidator<ImportarCotacoesRequest>
{
    public ImportarCotacoesValidator()
    {
        RuleFor(x => x.NomeArquivo)
            .NotEmpty().WithMessage("Nome do arquivo é obrigatório.")
            .Matches(@"^COTAHIST_[AD]\d{4,8}\.TXT$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .WithMessage("Nome do arquivo deve seguir o padrão COTAHIST_Dddmmaaaa.TXT ou COTAHIST_Aaaa.TXT.");
    }
}
