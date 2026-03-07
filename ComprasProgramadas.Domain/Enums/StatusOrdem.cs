namespace ComprasProgramadas.Domain.Enums;

/// <summary>
/// Ciclo de vida de uma ordem de compra consolidada na conta master.
/// RN passo 2 do motor: a ordem vai de Pendente → Executada (ou Erro).
/// </summary>
public enum StatusOrdem
{
    Pendente  = 1,
    Executada = 2,
    Erro      = 3
}
