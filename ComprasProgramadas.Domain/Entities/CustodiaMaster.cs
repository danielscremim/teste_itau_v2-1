using ComprasProgramadas.Domain.Exceptions;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Posição de um ativo na conta Master (resíduos após distribuição).
///
/// RN-039: ações não distribuídas por arredondamento ficam aqui.
/// RN-040: na próxima data de compra, esse saldo é descontado do total a comprar.
///
/// Por que existe?
/// Ex: sistema compra 30 ações de PETR4 e distribui 29 (truncamento).
/// 1 ação sobra → fica na master → é usada no próximo ciclo de compra.
/// </summary>
public class CustodiaMaster : EntidadeBase
{
    public string   Ticker                  { get; private set; } = string.Empty;
    public int      Quantidade              { get; private set; }
    public decimal  PrecoMedio              { get; private set; }
    public DateTime DataUltimaAtualizacao   { get; private set; }

    protected CustodiaMaster() { }

    public static CustodiaMaster Criar(string ticker)
    {
        return new CustodiaMaster
        {
            Ticker                = ticker.Trim().ToUpper(),
            Quantidade            = 0,
            PrecoMedio            = 0,
            DataUltimaAtualizacao = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adiciona ações residuais após uma distribuição (RN-039).
    /// O preço médio é recalculado a cada entrada — igual à regra da custódia filhote (RN-042).
    /// </summary>
    public void AdicionarResiduo(int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0) return;

        PrecoMedio = Quantidade == 0
            ? precoUnitario
            : (Quantidade * PrecoMedio + quantidade * precoUnitario) / (Quantidade + quantidade);

        Quantidade           += quantidade;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove ações da master quando são descontadas de uma nova compra (RN-030).
    /// </summary>
    public void Descontar(int quantidade)
    {
        if (quantidade > Quantidade)
            throw new DomainException($"Saldo insuficiente na custódia master para {Ticker}. " +
                                      $"Disponível: {Quantidade}, solicitado: {quantidade}.");

        Quantidade           -= quantidade;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }
}
