using CSnakes.Runtime;

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

builder.Services.AddScoped<IRootFinding>(sp => 
{
    var env = sp.GetRequiredService<IPythonEnvironment>();
    return env.RootFinding(); 
});

builder.Services.AddScoped<IFunctionBuilding>(sp => 
{
    var env = sp.GetRequiredService<IPythonEnvironment>();
    return env.FunctionBuilding(); 
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();