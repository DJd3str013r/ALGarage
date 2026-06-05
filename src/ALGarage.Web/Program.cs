using ALGarage.Domain.Maintenance;
using ALGarage.Infrastructure;
using ALGarage.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// --- Blazor Web App: render mode InteractiveServer no MVP (ADR-0002). ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Domínio: serviços puros. ---
builder.Services.AddSingleton<IMaintenanceEstimator, MaintenanceEstimator>();

// --- Infrastructure: EF Core (Postgres), decoders de VIN, providers de peças. ---
builder.Services.AddInfrastructure(builder.Configuration);

// TODO(feature): AddAuthentication/Identity (ADR-0004), Serilog, OpenTelemetry, health checks.

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
