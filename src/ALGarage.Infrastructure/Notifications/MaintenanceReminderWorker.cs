using ALGarage.Application.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALGarage.Infrastructure.Notifications;

/// <summary>Configuração do worker de lembretes (seção "Reminders"). Desabilitado por padrão.</summary>
public sealed class ReminderOptions
{
    public const string SectionName = "Reminders";

    public bool Enabled { get; set; }
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);
}

/// <summary>
/// Roda periodicamente e envia lembretes de manutenção por e-mail. Desabilitado por padrão
/// (ative a seção "Reminders" + configure "Email"). Em CI/dev não interfere: retorna na hora.
/// </summary>
public sealed class MaintenanceReminderWorker(
    IServiceProvider services,
    IOptions<ReminderOptions> options,
    ILogger<MaintenanceReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options.Value;
        if (!opts.Enabled)
        {
            logger.LogInformation("Worker de lembretes desabilitado (Reminders:Enabled=false).");
            return;
        }

        try
        {
            await Task.Delay(opts.InitialDelay, stoppingToken);
            using var timer = new PeriodicTimer(opts.Interval);
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // shutdown — esperado
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var notifier = scope.ServiceProvider.GetRequiredService<MaintenanceNotificationService>();
            var sent = await notifier.SendDueRemindersAsync(ct);
            logger.LogInformation("Lembretes de manutenção processados: {Count} e-mail(s).", sent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao processar lembretes de manutenção.");
        }
    }
}
