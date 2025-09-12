using EthanRushbrook.TempSense.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ClientHub>("/hub");

var webTask = Task.Run(() => app.Run());
var avaloniaTask = Task.Run(() => EthanRushbrook.TempSense.Program.Main([]));

// Wait until both have exited
await Task.WhenAny(webTask, avaloniaTask);

Environment.Exit(0);
