using CSnakes.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var home = Path.Join(Environment.CurrentDirectory, ".\\Scripts");

builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable()
    .WithPipInstaller();

builder.Services.AddScoped<IRootFinding>(sp => 
{
    var env = sp.GetRequiredService<IPythonEnvironment>();
    return env.RootFinding(); 
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