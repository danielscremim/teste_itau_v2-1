using ComprasProgramadas.Domain.Exceptions;

namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Representa a pessoa física que adere ao produto de compra programada.
///
/// Regras de negócio encapsuladas aqui (Domain logic):
/// - RN-002: CPF deve ser único (validado no repositório, mas o campo é readonly após criação)
/// - RN-003: Valor mensal mínimo R$ 100,00
/// - RN-005: Cliente inicia com Ativo = true
/// - RN-007: Ao sair, Ativo = false (método Desativar)
/// - RN-011: Alteração do valor mensal via método próprio (AlterarValorMensal)
/// </summary>
public class Cliente : EntidadeBase
{
    public string   Nome         { get; private set; } = string.Empty;
    public string   Cpf          { get; private set; } = string.Empty;
    public string   Email        { get; private set; } = string.Empty;
    public decimal  ValorMensal  { get; private set; }
    public bool     Ativo        { get; private set; }
    public DateTime DataAdesao   { get; private set; }
    public DateTime? DataSaida   { get; private set; }
    public DateTime UpdatedAt    { get; private set; }

    // Navegação: EF Core usa essas propriedades para montar os JOINs
    public ContaGrafica?                ContaGrafica              { get; private set; }
    public ICollection<CustodiaFilhote> CustodiasFilhote          { get; private set; } = [];
    public ICollection<HistoricoValorMensal> HistoricoValorMensal { get; private set; } = [];

    // EF Core exige construtor sem parâmetros (protegido para não ser usado fora)
    protected Cliente() { }

    /// <summary>
    /// Cria um novo cliente já com todos os invariantes de negócio garantidos.
    /// Factory method: centraliza a criação correta do objeto.
    /// </summary>
    public static Cliente Criar(string nome, string cpf, string email, decimal valorMensal)
    {
        // RN-003: valor mínimo R$ 100,00
        if (valorMensal < 100)
            throw new DomainException("O valor mensal mínimo é de R$ 100,00. (RN-003)");

        return new Cliente
        {
            Nome        = nome.Trim(),
            Cpf         = cpf.Trim(),
            Email       = email.Trim(),
            ValorMensal = valorMensal,
            Ativo       = true,       // RN-005
            DataAdesao  = DateTime.UtcNow, // RN-006
            UpdatedAt   = DateTime.UtcNow
        };
    }

    /// <summary>
    /// RN-007: Saída do produto — interrompe novas compras, mantém custódia.
    /// </summary>
    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("Cliente já está inativo.");

        Ativo     = false;
        DataSaida = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// RN-011/RN-012: Altera o valor mensal. O novo valor vale na próxima data de compra.
    /// Retorna o valor anterior para ser salvo no histórico (RN-013).
    /// </summary>
    public decimal AlterarValorMensal(decimal novoValor)
    {
        if (novoValor < 100)
            throw new DomainException("O valor mensal mínimo é de R$ 100,00. (RN-003)");

        var valorAnterior = ValorMensal;
        ValorMensal       = novoValor;
        UpdatedAt         = DateTime.UtcNow;
        return valorAnterior;
    }
}
