namespace ComprasProgramadas.Domain.Enums;

/// <summary>
/// Motivo pelo qual uma venda foi registrada no controle mensal de IR.
/// Usado para auditoria e rastreabilidade (RN-057).
/// </summary>
public enum OrigemVenda
{
    RebalanceamentoCesta   = 1,
    RebalanceamentoDesvio  = 2
}
