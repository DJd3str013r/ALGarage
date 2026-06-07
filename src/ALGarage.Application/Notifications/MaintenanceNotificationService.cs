using System.Text;
using ALGarage.Application.Abstractions;
using ALGarage.Domain.Common;
using ALGarage.Domain.Maintenance;
using ALGarage.Domain.Service;

namespace ALGarage.Application.Notifications;

/// <summary>
/// Monta e envia lembretes de manutenção (itens Vencidos/Próximos) por e-mail, varrendo todos os
/// veículos cujos donos têm e-mail. Usado pelo worker periódico. A lógica de "o que está pendente"
/// é testável (BuildRemindersAsync).
/// </summary>
public sealed class MaintenanceNotificationService(
    IReminderQueries reminderQueries,
    ICatalogRepository catalog,
    IGarageRepository garage,
    IMaintenanceEstimator estimator,
    IEmailSender email,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<UserReminder>> BuildRemindersAsync(CancellationToken ct = default)
    {
        var targets = await reminderQueries.GetTargetsAsync(ct);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var evaluator = new MaintenanceScheduleEvaluator(estimator);

        var byEmail = new Dictionary<string, List<VehicleReminder>>(StringComparer.OrdinalIgnoreCase);

        foreach (var target in targets)
        {
            var v = target.Vehicle;
            if (v.ModelVariantId is not { } variantId)
            {
                continue;
            }

            var variant = await catalog.GetVariantDetailAsync(variantId, ct);
            if (variant is null)
            {
                continue;
            }

            var plan = await catalog.GetPlanByEngineFamilyAsync(variant.EngineFamily, ct);
            if (plan is null || plan.Items.Count == 0)
            {
                continue;
            }

            var records = await garage.GetServiceRecordsAsync(v.Id, ct);
            var lastByItem = BuildLastServiceByItem(records);
            var context = new VehicleMaintenanceContext(
                v.PurchaseDate, v.OdometerAtAcquisitionKm, v.CurrentOdometerKm, v.AvgDailyKm);

            var due = evaluator.Evaluate(context, plan.Items, lastByItem, today)
                .Where(s => s.Evaluation.State != MaintenanceState.Ok)
                .Select(s => new DueItem(s.ItemName, s.Evaluation.State))
                .ToList();

            if (due.Count == 0)
            {
                continue;
            }

            var display = $"{variant.ModelName} {variant.Trim}";
            if (!byEmail.TryGetValue(target.Email, out var vehicles))
            {
                vehicles = [];
                byEmail[target.Email] = vehicles;
            }

            vehicles.Add(new VehicleReminder(display, due));
        }

        return byEmail.Select(kv => new UserReminder(kv.Key, kv.Value)).ToList();
    }

    /// <summary>Monta e envia os lembretes. Retorna quantos e-mails foram disparados.</summary>
    public async Task<int> SendDueRemindersAsync(CancellationToken ct = default)
    {
        var reminders = await BuildRemindersAsync(ct);
        foreach (var reminder in reminders)
        {
            await email.SendAsync(reminder.Email, "ÄLGarage — itens de manutenção pendentes", Compose(reminder), ct);
        }

        return reminders.Count;
    }

    private static string Compose(UserReminder reminder)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Olá! Estes itens do(s) seu(s) carro(s) precisam de atenção:");
        sb.AppendLine();
        foreach (var vehicle in reminder.Vehicles)
        {
            sb.AppendLine($"• {vehicle.Display}");
            foreach (var item in vehicle.Items)
            {
                var label = item.State == MaintenanceState.Overdue ? "VENCIDO" : "próximo";
                sb.AppendLine($"    - {item.Name} [{label}]");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Acesse o ÄLGarage para detalhes. (Você recebe este e-mail porque ativou os lembretes.)");
        return sb.ToString();
    }

    private static Dictionary<string, LastServiceForItem> BuildLastServiceByItem(IReadOnlyList<ServiceRecord> records)
    {
        var map = new Dictionary<string, LastServiceForItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var record in records.OrderByDescending(r => r.PerformedOn))
        {
            foreach (var item in record.Items)
            {
                if (item.MaintenanceItemKey is { } key && !map.ContainsKey(key))
                {
                    map[key] = new LastServiceForItem(record.PerformedOn, record.OdometerKm);
                }
            }
        }

        return map;
    }
}
