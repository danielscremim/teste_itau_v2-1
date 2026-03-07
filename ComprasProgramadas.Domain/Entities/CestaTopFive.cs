using ComprasProgramadas.Domain.Exceptions;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// A cesta de recomendação "Top Five": exatamente 5 ações com percentuais que somam 100%.
///
/// Regras encapsuladas:
/// - RN-014: exatamente 5 ações
/// - RN-015: soma dos percentuais = 100%
/// - RN-016: cada percentual > 0%
/// - RN-017: ao criar nova cesta, a anterior é desativada (feito via método Desativar)
/// - RN-018: apenas uma cesta ativa por vez (garantido no Use Case)
/// - RN-019: alteração dispara rebalanceamento (responsabilidade do Use Case)
/// </summary>
public class CestaTopFive : EntidadeBase
{
    public bool       Ativa            { get; private set; }
    public DateTime   DataAtivacao     { get; private set; }
    public DateTime?  DataDesativacao  { get; private set; }
    public string?    CriadoPor        { get; private set; }

    // Navegação: lista dos 5 itens (ação + percentual)
    public ICollection<ItemCesta> Itens { get; private set; } = [];

    protected CestaTopFive() { }

    /// <summary>
    /// Cria uma nova cesta já validada com os 5 ativos e percentuais corretos.
    /// </summary>
    public static CestaTopFive Criar(List<(string Ticker, decimal Percentual)> itens, string? criadoPor = null)
    {
        // RN-014: exatamente 5 ações
        if (itens.Count != 5)
            throw new DomainException("A cesta deve conter exatamente 5 ações. (RN-014)");

        // RN-016: cada percentual > 0%
        if (itens.Any(i => i.Percentual <= 0))
            throw new DomainException("Todos os percentuais devem ser maiores que 0%. (RN-016)");

        // RN-015: soma = 100%
        var soma = itens.Sum(i => i.Percentual);
        if (Math.Abs(soma - 100) > 0.001m)
            throw new DomainException($"A soma dos percentuais deve ser 100%. Soma atual: {soma}%. (RN-015)");

        var cesta = new CestaTopFive
        {
            Ativa           = true,
            DataAtivacao    = DateTime.UtcNow,
            DataDesativacao = null,
            CriadoPor       = criadoPor
        };

        cesta.Itens = itens.Select(i => ItemCesta.Criar(i.Ticker, i.Percentual)).ToList();
        return cesta;
    }

    /// <summary>
    /// RN-017: Desativa esta cesta quando uma nova é cadastrada.
    /// </summary>
    public void Desativar()
    {
        Ativa           = false;
        DataDesativacao = DateTime.UtcNow;
    }
}
