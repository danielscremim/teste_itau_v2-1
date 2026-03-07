namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Registra cada vez que um cliente alterou seu valor mensal de aporte.
/// Garante rastreabilidade total — exigência da RN-013.
/// 
/// Imutável após criação: histórico nunca deve ser alterado.
/// </summary>
public class HistoricoValorMensal : EntidadeBase
{
    public long     ClienteId      { get; private set; }
    public decimal  ValorAnterior  { get; private set; }
    public decimal  ValorNovo      { get; private set; }
    public DateTime DataAlteracao  { get; private set; }

    // Navegação
    public Cliente? Cliente { get; private set; }

    protected HistoricoValorMensal() { }

    public static HistoricoValorMensal Registrar(long clienteId, decimal valorAnterior, decimal valorNovo)
    {
        return new HistoricoValorMensal
        {
            ClienteId     = clienteId,
            ValorAnterior = valorAnterior,
            ValorNovo     = valorNovo,
            DataAlteracao = DateTime.UtcNow
        };
    }
}
