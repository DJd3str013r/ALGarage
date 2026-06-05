using System.Globalization;
using ALGarage.Application;
using ALGarage.Application.Abstractions;
using ALGarage.Infrastructure;
using ALGarage.Infrastructure.Identity;
using ALGarage.Infrastructure.Persistence;
using ALGarage.Web.Components;
using ALGarage.Web.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// --- Blazor Web App: render mode InteractiveServer disponível; telas da garagem usam SSR. ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- i18n: Inglês + Português (ADR-0013). ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("pt-BR") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// --- Camadas Application + Infrastructure. ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Autenticação (ASP.NET Core Identity, cookie). ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, HttpContextAuthenticationStateProvider>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // sem envio de e-mail no MVP
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Sem HSTS/HttpsRedirection: o MVP roda na LAN (Raspberry Pi) por HTTP (ver docs/08).
}

app.UseRequestLocalization(app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Logout (POST a partir do formulário do menu).
app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
    {
        await signInManager.SignOutAsync();
        return Results.LocalRedirect("/");
    })
    .DisableAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- Migrations + seed do catálogo curado (V40). Falha graciosamente se o banco não estiver pronto. ---
try
{
    await AppDbInitializer.RunAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex,
        "Inicialização do banco falhou. Gere a migration inicial e garanta o Postgres: " +
        "dotnet ef migrations add InitialCreate -p src/ALGarage.Infrastructure -s src/ALGarage.Web");
}

app.Run();
