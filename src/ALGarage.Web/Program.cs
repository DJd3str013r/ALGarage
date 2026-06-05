using System.Globalization;
using ALGarage.Domain.Maintenance;
using ALGarage.Infrastructure;
using ALGarage.Web.Components;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// --- Blazor Web App: render mode InteractiveServer no MVP (ADR-0002). ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- i18n: Inglês + Português desde o MVP (ADR-0013). Recursos .resx em /Resources. ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("pt-BR") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// --- Domínio: serviços puros. ---
builder.Services.AddSingleton<IMaintenanceEstimator, MaintenanceEstimator>();

// --- Infrastructure: EF Core (Postgres), decoders de VIN, providers de peças. ---
builder.Services.AddInfrastructure(builder.Configuration);

// TODO(feature): AddAuthentication/Identity (ADR-0004), Serilog, OpenTelemetry, health checks.

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Sem HSTS/HttpsRedirection: o MVP roda na LAN (Raspberry Pi) por HTTP.
    // TLS, se desejado, fica num reverse proxy à frente (ver docs/08-implantacao-local.md).
}

app.UseRequestLocalization(app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
