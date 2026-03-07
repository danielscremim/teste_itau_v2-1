using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.UseCases.Admin;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComprasProgramadas.API.Services;

/// <summary>
/// Serviço de agendamento automático do Motor de Compra Programada.
///
/// Executa nos dias úteis iguais ou subsequentes ao dia 5, 15 e 25 de cada mês (RN-020/RN-021).
/// Roda em background como IHostedService — inicia junto com a aplicação.
///
/// Por que IHostedService e não um CRON externo?
///   - Para este desafio, manter tudo no mesmo processo .NET simplifica a entrega.
///   - Em produção real, usaríamos Hangfire, Quartz.NET ou AWS EventBridge.
///
/// Ciclo: verifica a cada hora se é um dia de compra e se já executou hoje.
/// Idempotente: usa ExisteOrdemParaDataAsync para evitar execução duplicada.
/// </summary>
public class MotorCompraSchedulerService : BackgroundService
{
    // IServiceScopeFactory é necessário porque BackgroundService é Singleton
    // mas os Use Cases e repositórios são Scoped (por request HTTP).
    // Criamos um novo escopo DI manualmente a cada execução para não violar o ciclo de vida.
    private readonly IServiceScopeFactory                 _scopeFactory;
    private readonly ILogger<MotorCompraSchedulerService> _logger;

    // Intervalo de verificação. Em produção pode ser TimeSpan.FromHours(1).
    private static readonly TimeSpan VerificacaoInterval = TimeSpan.FromHours(1);

    public MotorCompraSchedulerService(
        IServiceScopeFactory                 scopeFactory,
        ILogger<MotorCompraSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MotorCompraScheduler iniciado. Verificando a cada {Interval}.",
            VerificacaoInterval);

        // Verificação imediata ao iniciar a aplicação
        await VerificarEExecutarAsync(stoppingToken);

        // Loop periódico
        using var timer = new PeriodicTimer(VerificacaoInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await VerificarEExecutarAsync(stoppingToken);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VERIFICAÇÃO E EXECUÇÃO
    // ─────────────────────────────────────────────────────────────────────────

    private async Task VerificarEExecutarAsync(CancellationToken ct)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        if (!EhDiaDeCompra(hoje))
        {
            _logger.LogDebug("Hoje ({Data}) não é dia de compra programada.", hoje);
            return;
        }

        using var scope   = _scopeFactory.CreateScope();
        var ordemRepo     = scope.ServiceProvider.GetRequiredService<IOrdemCompraRepository>();

        // Idempotência: verifica se o motor já rodou hoje para esta data de referência
        if (await ordemRepo.ExisteOrdemParaDataAsync(hoje))
        {
            _logger.LogInformation("Motor de compra já executado para {Data}. Pulando.", hoje);
            return;
        }

        _logger.LogInformation("Iniciando execução automática do motor de compra para {Data}.", hoje);

        try
        {
            var motorUseCase = scope.ServiceProvider.GetRequiredService<ExecutarMotorCompraUseCase>();
            var resultado    = await motorUseCase.ExecutarAsync(new ExecutarMotorCompraRequest(hoje));

            _logger.LogInformation(
                "Motor executado com sucesso. OrdemId={OrdemId}, Clientes={Clientes}, " +
                "Total=R${Total:N2}, Status={Status}.",
                resultado.OrdemId, resultado.TotalClientesAtivos,
                resultado.TotalConsolidado, resultado.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na execução automática do motor para {Data}.", hoje);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REGRAS DE DATA RN-020/RN-021/RN-022
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica se <paramref name="data"/> é um dia de compra programada.
    /// Dias base: 5, 15 e 25 de cada mês. Se cair no fim de semana, avança para segunda.
    /// </summary>
    internal static bool EhDiaDeCompra(DateOnly data)
    {
        foreach (var diaBase in new[] { 5, 15, 25 })
        {
            int diasNoMes = DateTime.DaysInMonth(data.Year, data.Month);
            if (diaBase > diasNoMes) continue;

            var dataBase = new DateOnly(data.Year, data.Month, diaBase);
            var dataUtil = ProximoDiaUtil(dataBase);
            if (dataUtil == data) return true;
        }
        return false;
    }

    /// <summary>
    /// Avança para o próximo dia útil (segunda–sexta), conforme RN-022.
    /// </summary>
    internal static DateOnly ProximoDiaUtil(DateOnly data)
    {
        while (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
            data = data.AddDays(1);
        return data;
    }
}
