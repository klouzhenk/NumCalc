using CSnakes.Runtime;
using NumCalc.Calculation.Api.HostedServices;
using NumCalc.Calculation.Api.Middlewares;
using NumCalc.Calculation.Api.Services.Implementations;
using NumCalc.Calculation.Api.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/api-log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "NumCalc API",
        Version = "v1",
        Description = "API for numerical calculation",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Name = "Yuliia",
            Email = "yuliavydryk@gmail.com"
        }
    });

    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var binPath = AppDomain.CurrentDomain.BaseDirectory;
var projectRoot = Path.GetFullPath(Path.Combine(binPath, "../../../../")); 
var venvPath = Path.Combine(projectRoot, ".venv");
var scriptsPath = Path.Combine(binPath, "Scripts");

builder.Services
    .WithPython()
    .WithHome(scriptsPath)
    .WithVirtualEnvironment(venvPath)
    .FromRedistributable()
    .WithPipInstaller(Path.Combine(scriptsPath, "requirements.txt"));

builder.Services.AddScoped<IRootFindingService, RootFindingService>();
builder.Services.AddScoped<IEquationsSystemService, EquationsSystemService>();
builder.Services.AddScoped<IInterpolationService, InterpolationService>();
builder.Services.AddScoped<IDifferentiationService, DifferentiationService>();
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IOptimizationService, OptimizationService>();
builder.Services.AddScoped<IOdeService, OdeService>();
builder.Services.AddHostedService<PythonWarmingUpService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();