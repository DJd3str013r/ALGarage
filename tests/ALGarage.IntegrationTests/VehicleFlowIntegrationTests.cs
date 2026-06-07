using ALGarage.Application.Abstractions;
using ALGarage.Application.Maintenance;
using ALGarage.Application.ServiceHistory;
using ALGarage.Application.Vehicles;
using ALGarage.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ALGarage.IntegrationTests;

/// <summary>
/// Teste de integração do fluxo principal (cadastro → estimativa → serviço reseta), rodando a
/// composição real (AddApplication + AddInfrastructure) contra um Postgres de verdade.
/// </summary>
[Collection(nameof(PostgresCollection))]
public sealed class VehicleFlowIntegrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Catalog_is_seeded_with_v40()
    {
        await using var provider = fixture.BuildProvider(Guid.NewGuid());
        using var scope = provider.CreateScope();
        var catalog = scope.ServiceProvider.GetRequiredService<ICatalogRepository>();

        (await catalog.HasAnyCatalogAsync()).ShouldBeTrue();
        (await catalog.FindVariantsByYearAsync(2016)).ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Register_then_estimate_then_service_resets_oil()
    {
        var userId = Guid.NewGuid();
        await using var provider = fixture.BuildProvider(userId);
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        var vehicles = sp.GetRequiredService<VehicleService>();
        var maintenance = sp.GetRequiredService<MaintenanceStatusService>();
        var history = sp.GetRequiredService<ServiceHistoryService>();

        // 1) Decodifica o VIN (Volvo, ano-modelo 2016) e confirma a versão.
        const string vin = "YV1MV7320G1234567";
        var decode = await vehicles.DecodeAsync(vin);
        decode.Identified.ShouldBeTrue();
        decode.ModelYear.ShouldBe(2016);
        var momentum = decode.CandidateVariants.First(c => c.Trim == "Momentum");

        // 2) Cadastra na garagem (km acima do intervalo de óleo e 2 anos desde a aquisição).
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var id = await vehicles.RegisterAsync(new RegisterVehicleRequest(
            Vin: vin,
            PurchaseDate: today.AddYears(-2),
            PreviousOwners: 1,
            OdometerAtAcquisitionKm: 30_000,
            CurrentOdometerKm: 60_000,
            AvgDailyKm: 40,
            Nickname: "V40 de teste",
            ModelVariantId: momentum.Id));

        // 3) Estimativa inicial: óleo vencido.
        var before = await maintenance.GetForVehicleAsync(id);
        before.HasPlan.ShouldBeTrue();
        before.Items.First(i => i.Category == "Óleo").State.ShouldBe(MaintenanceState.Overdue);

        // 4) Registra a troca de óleo hoje → deve zerar o relógio.
        await history.AddAsync(new AddServiceRecordRequest(
            VehicleId: id,
            PerformedOn: today,
            OdometerKm: 60_000,
            Workshop: "Oficina Teste",
            TotalCost: 450m,
            Notes: null,
            Items: [new AddServiceItemDto("Troca de óleo", "Troca de óleo e filtro de óleo", 450m)]));

        // 5) Reavalia: óleo volta a ficar em dia.
        var after = await maintenance.GetForVehicleAsync(id);
        after.Items.First(i => i.Category == "Óleo").State.ShouldBe(MaintenanceState.Ok);
    }
}
