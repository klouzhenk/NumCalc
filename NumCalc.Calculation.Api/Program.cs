using CSnakes.Runtime;
using NumCalc.Calculation.Api.Middlewares;
using NumCalc.Calculation.Api.Services.Implementations;
using NumCalc.Calculation.Api.Services.Interfaces;
using IRootFinding = NumCalc.Calculation.Api.Services.Interfaces.IRootFinding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var binPath = AppDomain.CurrentDomain.BaseDirectory;
var projectRoot = Path.GetFullPath(Path.Combine(binPath, "../../../../")); 
var venvPath = Path.Combine(projectRoot, ".venv");
var scriptsPath = Path.Combine(binPath, "Scripts");

builder.Services
    .WithPython()
    .WithHome(scriptsPath)
    .WithVirtualEnvironment(venvPath)
    .FromRedistributable()
    .WithPipInstaller();

builder.Services.AddScoped<CSnakes.Runtime.IRootFinding>(sp => 
{
    var env = sp.GetRequiredService<IPythonEnvironment>();
    return env.RootFinding(); 
});

builder.Services.AddScoped<IFunctionBuilding>(sp => 
{
    var env = sp.GetRequiredService<IPythonEnvironment>();
    return env.FunctionBuilding(); 
});

builder.Services.AddScoped<IRootFinding, RootFinding>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

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