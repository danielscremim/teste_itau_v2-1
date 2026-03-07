namespace ComprasProgramadas.Domain.Enums;

/// <summary>
/// O que disparou o rebalanceamento.
/// - MudancaCesta: administrador alterou a composição da Top Five (RN-045)
/// - DesvioProporcao: um ativo valorizou/desvalorizou além do limiar (RN-050)
/// </summary>
public enum TipoRebalanceamento
{
    MudancaCesta     = 1,
    DesvioProporcao  = 2
}
