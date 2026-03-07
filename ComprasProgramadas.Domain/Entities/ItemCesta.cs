namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Um dos 5 ativos que compõem a CestaTopFive, com seu percentual de alocação.
/// Entidade dependente — não existe sem uma cesta pai.
/// </summary>
public class ItemCesta : EntidadeBase
{
    public long    CestaId    { get; private set; }
    public string  Ticker     { get; private set; } = string.Empty;
    public decimal Percentual { get; private set; }

    // Navegação
    public CestaTopFive? Cesta { get; private set; }

    protected ItemCesta() { }

    public static ItemCesta Criar(string ticker, decimal percentual)
    {
        return new ItemCesta
        {
            Ticker     = ticker.Trim().ToUpper(),
            Percentual = percentual
        };
    }
}
