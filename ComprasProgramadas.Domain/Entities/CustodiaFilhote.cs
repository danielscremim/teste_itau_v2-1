using ComprasProgramadas.Domain.Exceptions;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Posição de um ativo na carteira individual do cliente.
///
/// Regras de negócio encapsuladas:
/// - RN-038: preço médio atualizado a cada compra
/// - RN-041: PM calculado por ativo por cliente
/// - RN-042: fórmula PM = (QtdAnt * PMAnt + QtdNova * PrecoNovo) / (QtdAnt + QtdNova)
/// - RN-043: venda NÃO altera o preço médio, apenas reduz a quantidade
/// - RN-044: PM só é recalculado em compras
/// </summary>
public class CustodiaFilhote : EntidadeBase
{
    public long     ClienteId               { get; private set; }
    public long     ContaGraficaId          { get; private set; }
    public string   Ticker                  { get; private set; } = string.Empty;
    public int      Quantidade              { get; private set; }
    public decimal  PrecoMedio              { get; private set; }
    public DateTime DataUltimaAtualizacao   { get; private set; }

    // Navegação
    public Cliente?      Cliente      { get; private set; }
    public ContaGrafica? ContaGrafica { get; private set; }

    protected CustodiaFilhote() { }

    public static CustodiaFilhote Criar(long clienteId, long contaGraficaId, string ticker)
    {
        return new CustodiaFilhote
        {
            ClienteId             = clienteId,
            ContaGraficaId        = contaGraficaId,
            Ticker                = ticker.Trim().ToUpper(),
            Quantidade            = 0,
            PrecoMedio            = 0,
            DataUltimaAtualizacao = DateTime.UtcNow
        };
    }

    /// <summary>
    /// RN-038/RN-042/RN-044: Registra uma compra e recalcula o preço médio ponderado.
    ///
    /// Exemplo:
    ///   Tinha 8 ações a PM R$ 35,00. Compra 10 a R$ 37,00.
    ///   Novo PM = (8×35 + 10×37) / 18 = R$ 36,11
    /// </summary>
    public void RegistrarCompra(int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade de compra deve ser maior que zero.");

        // RN-042: fórmula do preço médio ponderado
        PrecoMedio = Quantidade == 0
            ? precoUnitario
            : (Quantidade * PrecoMedio + quantidade * precoUnitario) / (Quantidade + quantidade);

        Quantidade           += quantidade;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// RN-043: Venda reduz a quantidade mas NÃO altera o preço médio.
    /// Retorna o lucro bruto da operação para cálculo de IR.
    /// </summary>
    public decimal RegistrarVenda(int quantidade, decimal precoVenda)
    {
        if (quantidade > Quantidade)
            throw new DomainException($"Quantidade insuficiente em custódia para {Ticker}. " +
                                      $"Disponível: {Quantidade}, solicitado: {quantidade}.");

        var lucro = quantidade * (precoVenda - PrecoMedio); // RN-060
        Quantidade           -= quantidade;
        DataUltimaAtualizacao = DateTime.UtcNow;
        return lucro; // pode ser negativo (prejuízo)
    }
}
