using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using ComprasProgramadas.Infrastructure.B3;
using ComprasProgramadas.Infrastructure.Data;
using ComprasProgramadas.Infrastructure.Messaging;
using ComprasProgramadas.Infrastructure.Persistence;
using ComprasProgramadas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComprasProgramadas.Infrastructure.Extensions;

/// <summary>
/// Método de extensão que registra todos os serviços de Infrastructure no sistema de DI.
///
/// O que é Injeção de Dependência (DI)?
/// É como uma lista telefônica:
///   Você liga para "IClienteRepository" (o nome)
///   O sistema vai buscar "ClienteRepository" (o número real)
///   e te entrega já pronto para usar.
///
/// Por que isso é útil?
/// Se você decidir trocar o banco de MySQL para PostgreSQL,
/// só cria um novo "ClienteRepositoryPostgres" e muda uma linha aqui.
/// Todo o resto do código continua igual — ninguém sabe que mudou.
///
/// Como usar: no Program.cs da API, chame apenas:
///   builder.Services.AddInfrastructure(builder.Configuration);
/// </summary>
public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── BANCO DE DADOS ───────────────────────────────────────────────
        // Conecta o EF Core ao MySQL usando a string de conexão do appsettings.json
        // ServerVersion.AutoDetect = detecta automaticamente a versão do MySQL
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=compras_programadas;User=root;Password=root;";

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mysqlOptions => mysqlOptions.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName)
            )
        );

        // ─── UNIT OF WORK ─────────────────────────────────────────────────
        // Scoped = uma instância por request HTTP (compartilhada entre repositórios)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ─── REPOSITÓRIOS ─────────────────────────────────────────────────
        // Cada linha: "quando alguém pedir a Interface → entrega a Implementação"
        services.AddScoped<IClienteRepository,          ClienteRepository>();
        services.AddScoped<ICestaTopFiveRepository,     CestaTopFiveRepository>();
        services.AddScoped<IContaGraficaRepository,     ContaGraficaRepository>();
        services.AddScoped<ICustodiaFilhoteRepository,  CustodiaFilhoteRepository>();
        services.AddScoped<ICustodiaMasterRepository,   CustodiaMasterRepository>();
        services.AddScoped<IOrdemCompraRepository,      OrdemCompraRepository>();
        services.AddScoped<IDistribuicaoRepository,     DistribuicaoRepository>();
        services.AddScoped<IVendaMesRepository,         VendaMesRepository>();
        services.AddScoped<ICotacaoHistoricaRepository, CotacaoHistoricaRepository>();
        services.AddScoped<IRebalanceamentoRepository,  RebalanceamentoRepository>();

        // ─── KAFKA ────────────────────────────────────────────────────────
        // Singleton = uma única instância para toda a aplicação
        // (o producer Kafka é thread-safe e deve ser reutilizado)
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();

        // ─── COTAHIST PARSER ─────────────────────────────────────────────
        // Transient = nova instância toda vez que for pedida (stateless)
        services.AddTransient<ICotahistParser, CotahistParser>();

        return services;
    }
}
