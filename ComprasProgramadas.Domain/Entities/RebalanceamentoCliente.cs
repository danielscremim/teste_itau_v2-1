using ComprasProgramadas.Domain.Enums;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Detalhe do rebalanceamento por cliente: quanto foi vendido, comprado e o IR devido.
///
/// RN-057/RN-059: se o total de vendas do cliente no mês ultrapassar R$ 20.000,
/// o sistema calcula 20% sobre o lucro líquido e publica no Kafka.
/// </summary>
public class RebalanceamentoCliente : EntidadeBase
{
    public long                  RebalanceamentoId  { get; private set; }
    public long                  ClienteId          { get; private set; }
    public StatusRebalanceamento Status             { get; private set; }
    public decimal               TotalVendas        { get; private set; }
    public decimal               TotalCompras       { get; private set; }
    public decimal               IrDevido           { get; private set; } // 20% sobre lucro se vendas > R$20k
    public bool                  KafkaIrPublicado   { get; private set; }
    public DateTime?             DataExecucao       { get; private set; }

    // Navegação
    public Rebalanceamento? Rebalanceamento { get; private set; }
    public Cliente?         Cliente         { get; private set; }

    protected RebalanceamentoCliente() { }

    public static RebalanceamentoCliente Criar(long rebalanceamentoId, long clienteId)
    {
        return new RebalanceamentoCliente
        {
            RebalanceamentoId = rebalanceamentoId,
            ClienteId         = clienteId,
            Status            = StatusRebalanceamento.Pendente,
            TotalVendas       = 0,
            TotalCompras      = 0,
            IrDevido          = 0,
            KafkaIrPublicado  = false
        };
    }

    public void RegistrarResultado(decimal totalVendas, decimal totalCompras, decimal irDevido)
    {
        TotalVendas  = totalVendas;
        TotalCompras = totalCompras;
        IrDevido     = irDevido;
        Status       = StatusRebalanceamento.Executado;
        DataExecucao = DateTime.UtcNow;
    }

    public void MarcarKafkaPublicado() => KafkaIrPublicado = true;
    public void MarcarErro()          => Status = StatusRebalanceamento.Erro;
}
