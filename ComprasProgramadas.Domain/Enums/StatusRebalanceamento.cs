namespace ComprasProgramadas.Domain.Enums;

/// <summary>
/// Ciclo de vida de um rebalanceamento (geral e por cliente).
/// </summary>
public enum StatusRebalanceamento
{
    Pendente  = 1,
    Executado = 2,
    Erro      = 3
}
