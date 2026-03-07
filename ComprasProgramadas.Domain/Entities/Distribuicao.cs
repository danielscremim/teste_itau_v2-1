namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Registro da alocação de ativos da conta master para a custódia filhote de um cliente.
///
/// - RN-034: distribuição proporcional ao aporte de cada cliente
/// - RN-035: proporção = AporteCliente / TotalConsolidado
/// - RN-036: quantidade = TRUNCAR(Proporção × QuantidadeTotalDisponível)
/// - RN-053/RN-054: IR dedo-duro = 0,005% sobre ValorOperacao por operação
/// - RN-055: publicar no Kafka
/// </summary>
public class Distribuicao : EntidadeBase
{
    public long     OrdemId            { get; private set; }
    public long     ClienteId          { get; private set; }
    public string   Ticker             { get; private set; } = string.Empty;
    public int      Quantidade         { get; private set; }
    public decimal  PrecoUnitario      { get; private set; }
    public decimal  ValorOperacao      { get; private set; }  // Quantidade × PrecoUnitario
    public decimal  ProporcaoCliente   { get; private set; }  // AporteCliente / TotalConsolidado
    public decimal  ValorIrDedoDuro    { get; private set; }  // 0,005% × ValorOperacao
    public bool     KafkaPublicado     { get; private set; }
    public DateTime DataDistribuicao   { get; private set; }

    // Navegação
    public OrdemCompra? Ordem   { get; private set; }
    public Cliente?     Cliente { get; private set; }

    protected Distribuicao() { }

    public static Distribuicao Criar(
        long    ordemId,
        long    clienteId,
        string  ticker,
        int     quantidade,
        decimal precoUnitario,
        decimal proporcaoCliente)
    {
        var valorOperacao = quantidade * precoUnitario;

        // RN-053: IR dedo-duro = 0,005% sobre o valor da operação
        // 0,005% = 0,00005 como decimal
        var valorIr = Math.Round(valorOperacao * 0.00005m, 2);

        return new Distribuicao
        {
            OrdemId          = ordemId,
            ClienteId        = clienteId,
            Ticker           = ticker.ToUpper(),
            Quantidade       = quantidade,
            PrecoUnitario    = precoUnitario,
            ValorOperacao    = valorOperacao,
            ProporcaoCliente = proporcaoCliente,
            ValorIrDedoDuro  = valorIr,
            KafkaPublicado   = false,
            DataDistribuicao = DateTime.UtcNow
        };
    }

    public void MarcarKafkaPublicado() => KafkaPublicado = true;
}
