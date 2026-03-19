using NumCalc.UI.Shared.HttpServices.Implementations;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Layouts;
using NumCalc.UI.Shared.Services.Implementations;
using NumCalc.UI.Shared.Services.Interfaces;
using WebUI.Components;
using NumCalc.UI.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

const string baseApiUrl = "http://localhost:5229";
builder.Services.AddHttpClient<ICalculationApiService, CalculationApiService>(client =>
{
    client.BaseAddress = new Uri(baseApiUrl);
});
builder.Services.AddHttpClient<IOcrService, OcrService>(client =>
{
    client.BaseAddress = new Uri(baseApiUrl);
});

builder.Services.AddScoped<IUiStateService, UiStateService>();

builder.Services.AddNumCalcUiShared();

var app = builder.Build();

var supportedCultures = new[] { "en", "uk" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("uk")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();


app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(MainLayout).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();