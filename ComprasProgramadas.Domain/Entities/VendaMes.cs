using ComprasProgramadas.Domain.Enums;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Acumula todas as vendas de um cliente em um determinado mês.
/// Necessário para verificar se o total ultrapassa R$ 20.000 (RN-057/RN-058/RN-059).
///
/// Uma linha por operação de venda. Para calcular o total do mês,
/// o sistema soma ValorTotalVenda agrupado por (ClienteId, MesReferencia).
/// </summary>
public class VendaMes : EntidadeBase
{
    public long        ClienteId       { get; private set; }
    public string      MesReferencia   { get; private set; } = string.Empty; // "YYYY-MM"
    public string      Ticker          { get; private set; } = string.Empty;
    public int         Quantidade      { get; private set; }
    public decimal     PrecoVenda      { get; private set; }
    public decimal     PrecoMedio      { get; private set; } // PM no momento da venda (RN-043)
    public decimal     ValorTotalVenda { get; private set; } // Quantidade × PrecoVenda
    public decimal     Lucro           { get; private set; } // RN-060: qtd × (precoVenda - PM)
    public OrigemVenda Origem          { get; private set; }
    public DateTime    DataVenda       { get; private set; }

    // Navegação
    public Cliente? Cliente { get; private set; }

    protected VendaMes() { }

    public static VendaMes Registrar(
        long       clienteId,
        string     ticker,
        int        quantidade,
        decimal    precoVenda,
        decimal    precoMedio,
        OrigemVenda origem)
    {
        var mesRef = DateTime.UtcNow.ToString("yyyy-MM");

        return new VendaMes
        {
            ClienteId       = clienteId,
            MesReferencia   = mesRef,
            Ticker          = ticker.ToUpper(),
            Quantidade      = quantidade,
            PrecoVenda      = precoVenda,
            PrecoMedio      = precoMedio,
            ValorTotalVenda = quantidade * precoVenda,
            Lucro           = quantidade * (precoVenda - precoMedio), // pode ser negativo
            Origem          = origem,
            DataVenda       = DateTime.UtcNow
        };
    }
}
