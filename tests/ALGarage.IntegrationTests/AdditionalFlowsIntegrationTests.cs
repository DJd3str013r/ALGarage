using ALGarage.Application.Maintenance;
using ALGarage.Application.Notifications;
using ALGarage.Application.Parts;
using ALGarage.Application.ServiceHistory;
using ALGarage.Application.Vehicles;
using ALGarage.Domain.Common;
using ALGarage.Infrastructure.Identity;
using ALGarage.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ALGarage.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public sealed class AdditionalFlowsIntegrationTests(PostgresFixture fixture)
{
    private const string Vin2016 = "YV1MV7320G1234567";

    [Fact]
    public async Task Parts_search_builds_links()
    {
        await using var provider = fixture.BuildProvider(Guid.NewGuid());
        using var scope = provider.CreateScope();
        var parts = scope.ServiceProvider.GetRequiredService<PartsLinkService>();

        var links = await parts.SearchAsync("filtro de óleo Volvo V40");

        links.ShouldNotBeEmpty();
        links[0].Url.ShouldContain("mercadolivre");
    }

    [Fact]
    public async Task Diesel_variant_resolves_diesel_plan_with_fuel_filter()
    {
        var userId = Guid.NewGuid();
        await using var provider = fixture.BuildProvider(userId);
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;
        var vehicles = sp.GetRequiredService<VehicleService>();
        var maintenance = sp.GetRequiredService<MaintenanceStatusService>();

        var decode = await vehicles.DecodeAsync(Vin2016);
        var d3 = decode.CandidateVariants.First(c => c.Trim == "D3"); // diesel (D4204T)

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var id = await vehicles.RegisterAsync(new RegisterVehicleRequest(
            Vin2016, today.AddYears(-1), 0, 10_000, 25_000, 30, "Diesel", d3.Id));

        var status = await maintenance.GetForVehicleAsync(id);
        status.HasPlan.ShouldBeTrue();
        status.Items.ShouldContain(i => i.ItemName.Contains("combustível", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Latest_service_wins_when_resetting_the_clock()
    {
        var userId = Guid.NewGuid();
        await using var provider = fixture.BuildProvider(userId);
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;
        var vehicles = sp.GetRequiredService<VehicleService>();
        var history = sp.GetRequiredService<ServiceHistoryService>();
        var maintenance = sp.GetRequiredService<MaintenanceStatusService>();

        var decode = await vehicles.DecodeAsync(Vin2016);
        var momentum = decode.CandidateVariants.First(c => c.Trim == "Momentum");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var id = await vehicles.RegisterAsync(new RegisterVehicleRequest(
            Vin2016, today.AddYears(-2), 1, 30_000, 60_000, 40, "Momentum", momentum.Id));

        const string oil = "Troca de óleo e filtro de óleo";

        // Serviço ANTIGO (13 meses atrás): ainda vencido.
        await history.AddAsync(new AddServiceRecordRequest(
            id, today.AddMonths(-13), 31_000, "Antiga", null, null,
            [new AddServiceItemDto("Óleo (antigo)", oil, null)]));
        (await maintenance.GetForVehicleAsync(id)).Items.First(i => i.Category == "Óleo")
            .State.ShouldBe(MaintenanceState.Overdue);

        // Serviço RECENTE (hoje): o mais novo vence e zera o relógio.
        await history.AddAsync(new AddServiceRecordRequest(
            id, today, 60_000, "Recente", null, null,
            [new AddServiceItemDto("Óleo (novo)", oil, null)]));
        (await maintenance.GetForVehicleAsync(id)).Items.First(i => i.Category == "Óleo")
            .State.ShouldBe(MaintenanceState.Ok);
    }

    [Fact]
    public async Task Reminders_include_user_with_overdue_vehicle()
    {
        var userId = Guid.NewGuid();
        const string emailAddr = "dono.teste@example.com";
        await using var provider = fixture.BuildProvider(userId);
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Cria o usuário Identity (com e-mail) — o lembrete precisa do e-mail do dono.
        var db = sp.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new ApplicationUser { Id = userId, Email = emailAddr, UserName = emailAddr });
        await db.SaveChangesAsync();

        var vehicles = sp.GetRequiredService<VehicleService>();
        var decode = await vehicles.DecodeAsync(Vin2016);
        var momentum = decode.CandidateVariants.First(c => c.Trim == "Momentum");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await vehicles.RegisterAsync(new RegisterVehicleRequest(
            Vin2016, today.AddYears(-2), 1, 30_000, 60_000, 40, "Momentum", momentum.Id));

        var notifier = sp.GetRequiredService<MaintenanceNotificationService>();
        var reminders = await notifier.BuildRemindersAsync();

        var mine = reminders.FirstOrDefault(r => r.Email == emailAddr);
        mine.ShouldNotBeNull();
        mine!.Vehicles.SelectMany(v => v.Items).ShouldContain(i => i.State == MaintenanceState.Overdue);

        // Envio (e-mail desabilitado apenas loga) dispara ao menos 1 digest.
        (await notifier.SendDueRemindersAsync()).ShouldBeGreaterThanOrEqualTo(1);
    }
}
