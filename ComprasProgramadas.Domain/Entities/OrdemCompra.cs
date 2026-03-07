using ComprasProgramadas.Domain.Enums;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Registro da compra consolidada executada na conta master.
/// Uma ordem é criada a cada data de execução (dia 5, 15 ou 25).
///
/// RN-020 a RN-026: o motor agrupa todos os clientes ativos,
/// calcula 1/3 do valor mensal de cada um, soma tudo e executa uma única compra.
/// </summary>
public class OrdemCompra : EntidadeBase
{
    public long         CestaId            { get; private set; }
    public DateTime     DataExecucao       { get; private set; }
    public DateOnly     DataReferencia     { get; private set; } // dia 5, 15 ou 25
    public decimal      TotalConsolidado   { get; private set; }
    public StatusOrdem  Status             { get; private set; }
    public string?      ArquivoCotacao     { get; private set; }

    // Navegação
    public CestaTopFive?              Cesta  { get; private set; }
    public ICollection<ItemOrdemCompra> Itens { get; private set; } = [];

    protected OrdemCompra() { }

    public static OrdemCompra Criar(long cestaId, DateOnly dataReferencia, decimal totalConsolidado, string? arquivoCotacao)
    {
        return new OrdemCompra
        {
            CestaId          = cestaId,
            DataReferencia   = dataReferencia,
            DataExecucao     = DateTime.UtcNow,
            TotalConsolidado = totalConsolidado,
            Status           = StatusOrdem.Pendente,
            ArquivoCotacao   = arquivoCotacao
        };
    }

    public void MarcarExecutada() => Status = StatusOrdem.Executada;
    public void MarcarErro()      => Status = StatusOrdem.Erro;
}
