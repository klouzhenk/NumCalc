using NumCalc.UI.Shared.HttpServices.Implementations;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Layouts;
using NumCalc.UI.Shared.Services.Implementations;
using NumCalc.UI.Shared.Services.Interfaces;
using WebUI.Components;

var builder = WebApplication.CreateBuilder(args);
const string baseApiUrl = "http://localhost:5229";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ICalculationApiService, CalculationApiService>(client =>
{
    client.BaseAddress = new Uri(baseApiUrl);
});

builder.Services.AddScoped<IUiStateService, UiStateService>();

var app = builder.Build();

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