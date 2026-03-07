using ComprasProgramadas.Domain.Enums;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Representa um evento de rebalanceamento da carteira.
/// Pode ser disparado por mudança de cesta (RN-045) ou desvio de proporção (RN-050).
/// Um rebalanceamento afeta todos os clientes ativos.
/// </summary>
public class Rebalanceamento : EntidadeBase
{
    public TipoRebalanceamento    Tipo          { get; private set; }
    public long?                  CestaNovaId   { get; private set; } // preenchido em MudancaCesta
    public StatusRebalanceamento  Status        { get; private set; }
    public DateTime               DataInicio    { get; private set; }
    public DateTime?              DataFim       { get; private set; }

    // Navegação
    public CestaTopFive?                         CestaNova { get; private set; }
    public ICollection<RebalanceamentoCliente>   Clientes  { get; private set; } = [];

    protected Rebalanceamento() { }

    public static Rebalanceamento CriarPorMudancaCesta(long cestaNovaId)
    {
        return new Rebalanceamento
        {
            Tipo        = TipoRebalanceamento.MudancaCesta,
            CestaNovaId = cestaNovaId,
            Status      = StatusRebalanceamento.Pendente,
            DataInicio  = DateTime.UtcNow
        };
    }

    public static Rebalanceamento CriarPorDesvio()
    {
        return new Rebalanceamento
        {
            Tipo       = TipoRebalanceamento.DesvioProporcao,
            Status     = StatusRebalanceamento.Pendente,
            DataInicio = DateTime.UtcNow
        };
    }

    public void MarcarExecutado() { Status = StatusRebalanceamento.Executado; DataFim = DateTime.UtcNow; }
    public void MarcarErro()      { Status = StatusRebalanceamento.Erro;      DataFim = DateTime.UtcNow; }
}
