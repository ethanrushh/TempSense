using System;
using Avalonia;
using System.Threading.Tasks;
using EthanRushbrook.TempSense;
using Microsoft.AspNetCore.Builder;
using EthanRushbrook.TempSense.Hubs;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ClientHub>("/hub");

var webTask = Task.Run(() => app.Run());
var avaloniaTask = Task.Run(() =>
{
    TempSenseApp.ServiceProvider = app.Services;
    
    AppBuilder.Configure<TempSenseApp>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .StartWithClassicDesktopLifetime(args);
});

// Wait until both have exited
await Task.WhenAny(webTask, avaloniaTask);

Environment.Exit(0);
