using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ComprasProgramadas.Infrastructure.Data;

/// <summary>
/// O DbContext é o "controle remoto" do banco de dados.
/// Cada DbSet&lt;T&gt; representa uma tabela — é por onde o EF Core
/// lê e escreve dados naquela tabela.
///
/// Exemplo prático:
///   _context.Clientes.Add(novoCliente)  →  INSERT INTO clientes ...
///   _context.Clientes.ToList()          →  SELECT * FROM clientes
///   await _context.SaveChangesAsync()   →  COMMIT (envia tudo ao banco)
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Cada linha abaixo = uma tabela no banco
    public DbSet<Cliente>                  Clientes                  { get; set; }
    public DbSet<HistoricoValorMensal>     HistoricoValorMensal      { get; set; }
    public DbSet<ContaGrafica>             ContasGraficas            { get; set; }
    public DbSet<CestaTopFive>             CestasTopFive             { get; set; }
    public DbSet<ItemCesta>                ItensCesta                { get; set; }
    public DbSet<CustodiaMaster>           CustodiaMaster            { get; set; }
    public DbSet<CustodiaFilhote>          CustodiaFilhote           { get; set; }
    public DbSet<OrdemCompra>              OrdensCompra              { get; set; }
    public DbSet<ItemOrdemCompra>          ItensOrdemCompra          { get; set; }
    public DbSet<Distribuicao>             Distribuicoes             { get; set; }
    public DbSet<VendaMes>                 VendasMes                 { get; set; }
    public DbSet<CotacaoHistorica>         CotacoesHistoricas        { get; set; }
    public DbSet<Rebalanceamento>          Rebalanceamentos          { get; set; }
    public DbSet<RebalanceamentoCliente>   RebalanceamentoClientes   { get; set; }

    /// <summary>
    /// OnModelCreating é chamado uma vez na inicialização.
    /// Aqui falamos ao EF Core: "a tabela de Cliente se chama 'clientes',
    /// a coluna CPF tem tamanho 11, tem índice único, etc."
    /// São as regras de como mapear o C# para o SQL.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automaticamente todos os arquivos de configuração
        // que estiverem na mesma assembly (pasta Configurations/)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
