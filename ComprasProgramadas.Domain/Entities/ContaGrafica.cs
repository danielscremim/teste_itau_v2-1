using ComprasProgramadas.Domain.Enums;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Conta gráfica: pode ser Master (da corretora) ou Filhote (de cada cliente).
///
/// - A conta Master é única no sistema e consolida as compras antes da distribuição.
/// - Cada cliente recebe uma conta Filhote ao aderir (RN-004).
/// - O número da conta segue o padrão: MST-000001 (Master) ou FLH-000001 (Filhote).
/// </summary>
public class ContaGrafica : EntidadeBase
{
    public string     NumeroConta  { get; private set; } = string.Empty;
    public TipoConta  Tipo         { get; private set; }
    public long?      ClienteId    { get; private set; } // null para conta Master
    public DateTime   DataCriacao  { get; private set; }

    // Navegação
    public Cliente?                     Cliente           { get; private set; }
    public ICollection<CustodiaFilhote> CustodiasFilhote  { get; private set; } = [];

    protected ContaGrafica() { }

    public static ContaGrafica CriarMaster()
    {
        return new ContaGrafica
        {
            NumeroConta = "MST-000001",
            Tipo        = TipoConta.Master,
            ClienteId   = null,
            DataCriacao = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria a conta filhote vinculada ao cliente. Chamado automaticamente na adesão (RN-004).
    /// O número sequencial é gerado pela camada de Application com base no total de contas existentes.
    /// </summary>
    public static ContaGrafica CriarFilhote(long clienteId, string numeroConta)
    {
        return new ContaGrafica
        {
            NumeroConta = numeroConta,
            Tipo        = TipoConta.Filhote,
            ClienteId   = clienteId,
            DataCriacao = DateTime.UtcNow
        };
    }
}
