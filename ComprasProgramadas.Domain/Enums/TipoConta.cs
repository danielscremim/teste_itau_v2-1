namespace ComprasProgramadas.Domain.Enums;

/// <summary>
/// Define se uma conta gráfica pertence à corretora (Master)
/// ou a um cliente específico (Filhote).
/// RN-004: ao aderir, o cliente recebe uma conta Filhote.
/// </summary>
public enum TipoConta
{
    Master  = 1,
    Filhote = 2
}
