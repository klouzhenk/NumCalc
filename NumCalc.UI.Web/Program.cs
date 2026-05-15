using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using NumCalc.UI.Shared.Extensions;
using NumCalc.UI.Shared.Layouts;
using NumCalc.UI.Shared.Services.Interfaces;
using WebUI.Components;
using WebUI.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

var dpBlobConnection = builder.Configuration["DataProtection:BlobConnectionString"];
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("NumCalc.UI.Web");

if (!string.IsNullOrWhiteSpace(dpBlobConnection))
{
    // Persist DP keys to Blob Storage so they survive container restarts.
    // If unset (local dev), falls back to the default per-instance ephemeral keys.
    dataProtection.PersistKeysToAzureBlobStorage(
        dpBlobConnection,
        containerName: "dataprotection-keys",
        blobName: "numcalc-ui-web-keys.xml");
}

builder.Services.AddScoped<ICultureService, CultureService>();
builder.Services.AddScoped<ITokenStorage, ProtectedTokenStorage>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddNumCalcUiSharedServices()
    .AddSharedLogging("Logs/web-log-.txt");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddHttpContextAccessor();
builder.Services.AddCalculationApiServices(builder.Configuration);
builder.Services.AddUserApiServices(builder.Configuration);


var app = builder.Build();

var supportedCultures = new[] { "en", "uk" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("uk")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();
app.MapGet("/set-culture", (string culture, string redirectUri, HttpContext context) =>
{
    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
    );

    return Results.LocalRedirect(redirectUri);
});

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(MainLayout).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();