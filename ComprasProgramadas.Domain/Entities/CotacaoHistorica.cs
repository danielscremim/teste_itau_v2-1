namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Armazena os dados de cotação de um ativo em um pregão específico,
/// vindos do arquivo COTAHIST da B3.
///
/// RN-027: a cotação utilizada para cálculo das compras é o PrechoFechamento
/// do último pregão disponível na pasta cotacoes/.
///
/// O campo ArquivoOrigem permite rastrear de qual arquivo TXT cada cotação veio.
/// </summary>
public class CotacaoHistorica : EntidadeBase
{
    public string   Ticker           { get; private set; } = string.Empty;
    public DateOnly DataPregao       { get; private set; }
    public decimal  PrecoAbertura    { get; private set; }
    public decimal  PrecoMaximo      { get; private set; }
    public decimal  PrecoMinimo      { get; private set; }
    public decimal  PrecoFechamento  { get; private set; }
    public decimal? PrecoMedioDia    { get; private set; }
    public decimal? VolumeNegociado  { get; private set; }
    public string   ArquivoOrigem    { get; private set; } = string.Empty;

    protected CotacaoHistorica() { }

    public static CotacaoHistorica Criar(
        string  ticker,
        DateOnly dataPregao,
        decimal precoAbertura,
        decimal precoMaximo,
        decimal precoMinimo,
        decimal precoFechamento,
        string  arquivoOrigem,
        decimal? precoMedioDia    = null,
        decimal? volumeNegociado  = null)
    {
        return new CotacaoHistorica
        {
            Ticker          = ticker.Trim().ToUpper(),
            DataPregao      = dataPregao,
            PrecoAbertura   = precoAbertura,
            PrecoMaximo     = precoMaximo,
            PrecoMinimo     = precoMinimo,
            PrecoFechamento = precoFechamento,
            PrecoMedioDia   = precoMedioDia,
            VolumeNegociado = volumeNegociado,
            ArquivoOrigem   = arquivoOrigem
        };
    }
}
