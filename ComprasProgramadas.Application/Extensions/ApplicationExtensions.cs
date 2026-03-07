using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ComprasProgramadas.Application.UseCases.Admin;
using ComprasProgramadas.Application.UseCases.Clientes;

namespace ComprasProgramadas.Application.Extensions;

/// <summary>
/// Registra todos os Use Cases e Validators no container de DI.
/// 
/// DI (Dependency Injection) = o sistema que entrega os objetos certos para cada classe.
/// Quando o Controller pede um AderirAoProdutoUseCase, o .NET cria um automaticamente
/// com todos os repositórios já "plugados".
/// </summary>
public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ──────────────────────────────────────────────
        // VALIDATORS (FluentValidation)
        // Escaneia toda a assembly e registra todos os AbstractValidator<T>
        // Usamos typeof().Assembly pois a classe é estática e não aceita parâmetro de tipo genérico
        // ──────────────────────────────────────────────
        services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);

        // ──────────────────────────────────────────────
        // USE CASES DE CLIENTES
        // ──────────────────────────────────────────────
        services.AddScoped<AderirAoProdutoUseCase>();
        services.AddScoped<SairDoProdutoUseCase>();
        services.AddScoped<AlterarValorMensalUseCase>();
        services.AddScoped<ConsultarCarteiraUseCase>();
        services.AddScoped<ConsultarRentabilidadeUseCase>();

        // ──────────────────────────────────────────────
        // USE CASES DE ADMIN
        // ──────────────────────────────────────────────
        services.AddScoped<CadastrarCestaTopFiveUseCase>();
        services.AddScoped<ObterCestaAtivaUseCase>();
        services.AddScoped<ListarHistoricoCestasUseCase>();
        services.AddScoped<ImportarCotacoesUseCase>(sp =>
        {
            // ImportarCotacoesUseCase precisa da pasta de cotações como parâmetro
            // Lemos do appsettings.json: "CotacoesPath": "C:\\...\\cotacoes"
            var pastaCotacoes = configuration["CotacoesPath"]
                ?? Path.Combine(Directory.GetCurrentDirectory(), "cotacoes");

            return new ImportarCotacoesUseCase(
                sp.GetRequiredService<Domain.Interfaces.ICotahistParser>(),
                sp.GetRequiredService<Domain.Interfaces.Repositories.ICotacaoHistoricaRepository>(),
                sp.GetRequiredService<Domain.Interfaces.IUnitOfWork>(),
                pastaCotacoes
            );
        });
        services.AddScoped<ExecutarMotorCompraUseCase>(sp => new ExecutarMotorCompraUseCase(
            sp.GetRequiredService<Domain.Interfaces.Repositories.IClienteRepository>(),
            sp.GetRequiredService<Domain.Interfaces.Repositories.ICestaTopFiveRepository>(),
            sp.GetRequiredService<Domain.Interfaces.Repositories.ICotacaoHistoricaRepository>(),
            sp.GetRequiredService<Domain.Interfaces.Repositories.ICustodiaMasterRepository>(),
            sp.GetRequiredService<Domain.Interfaces.Repositories.ICustodiaFilhoteRepository>(),
            sp.GetRequiredService<Domain.Interfaces.Repositories.IContaGraficaRepository>(),
            sp.GetRequiredService<Domain.Interfaces.Repositories.IOrdemCompraRepository>(),
            sp.GetRequiredService<Domain.Interfaces.Repositories.IDistribuicaoRepository>(),
            sp.GetRequiredService<Domain.Interfaces.IKafkaPublisher>(),
            sp.GetRequiredService<Domain.Interfaces.IUnitOfWork>()
        ));

        // ── Rebalanceamento ────────────────────────────────────────────────────
        services.AddScoped<ExecutarRebalanceamentoUseCase>();

        return services;
    }
}
