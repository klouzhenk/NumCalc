using Microsoft.AspNetCore.Localization;
using NumCalc.UI.Shared.HttpServices.Implementations;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Layouts;
using NumCalc.UI.Shared.Services.Implementations;
using NumCalc.UI.Shared.Services.Interfaces;
using WebUI.Components;
using NumCalc.UI.Shared.Extensions;
using WebUI.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ICultureService, CultureService>();
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