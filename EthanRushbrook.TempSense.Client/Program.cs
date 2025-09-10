using System.Text.Json;
using EthanRushbrook.TempSense.Client;
using EthanRushbrook.TempSense.Client.Sensors;
using EthanRushbrook.TempSense.Contracts;
using Microsoft.AspNetCore.SignalR.Client;



// Detects the platform we're on, gets the sensor engine for that platform. Very cool :)
using var sensors = SystemSensors.GetAutoSensors(PlatformDetection.DetectOsPlatform());

foreach (var arg in args)
{
    switch (arg)
    {
        case "--list-sensors":
            sensors.ListSensors();
            return;
    }
}


var configFile = File.OpenRead("config.json");

var jsonDocument = await JsonDocument.ParseAsync(configFile);

configFile.Close();
await configFile.DisposeAsync();

var config = jsonDocument.Deserialize<ConfigModel>();

if (config is null)
    throw new JsonException("Invalid config file");

config.ValidateOrThrow();

Console.WriteLine("DEBUG: Parsed config file");


if (config.Widgets is null)
    throw new Exception("No widgets set in config");
var widgets = config.Widgets.Select(x => (Widget: new WidgetDefinition
{
    Id = Guid.NewGuid(),
    Header = x.DisplayType == WidgetDisplayType.Round ? GetWidgetValue(x) + x.Unit : x.Header,
    DisplayType = x.DisplayType ?? throw new Exception("Invalid display type in config"),
    Caption = x.DisplayName ?? "UNKNOWN",
    InitialValue = GetWidgetValue(x)
}, Definition: x)).ToList();


var connection = new HubConnectionBuilder()
    .WithUrl(config.ServerEndpoint ?? throw new Exception("No server endpoint set in config"))
    .Build();

// Restart connection if closed
connection.Closed += async _ =>
{
    await Task.Delay(1000);
    await connection.StartAsync();
};

connection.On("HelloFromServer", () => Console.WriteLine("Server says hello"));

// Set up widget page on the server
connection.On("RequestPageDefinition", async () =>
{
    await connection.SendAsync("ReceivePageDefinition", config.PageId, widgets.Select(x => x.Widget), config.WidgetLayout);
});

// Connect to server until successful
while (true)
{
    try
    {
        await connection.StartAsync();
        
        Console.WriteLine("Connected to server");

        break;
    }
    catch (Exception ex) { Console.WriteLine($"Couldn't connect to the server because {ex.Message}, retrying."); await Task.Delay(1000);}
}

// Periodic timers are great because they'll adjust the delay to keep things in time
var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.5));

// Main loop
do
{
    var dataPoints = widgets.Select(x => new DataPoint
    {
        WidgetId = x.Widget.Id,
        NewValue = GetWidgetValue(x.Definition),
        NewHeader = x.Definition.Type == WidgetType.Sensor
            ? GetWidgetValue(x.Definition) + x.Definition.Unit
            : x.Widget.Header
    }).ToList();

    if (connection.State == HubConnectionState.Connected)
        await connection.SendAsync("ReceiveDataframe", new Dataframe
        {
            PageId = config.PageId ?? throw new Exception("No page ID is set in config"),
            DataPoints = dataPoints
        });

} while (await timer.WaitForNextTickAsync());




string GetWidgetValue(ConfigWidgetDefinition widget)
{
    switch (widget.Type)
    {
        case WidgetType.Network:
            var networkStats = sensors.GetNetworkStats(widget.DeviceName ??
                                                       throw new Exception("Device name missing for widget " +
                                                                           widget.DisplayName));

            var speed = widget.Direction == NetworkDirection.Down ? networkStats.DownloadSpeed : networkStats.UploadSpeed;
            return $"{speed * 8 / 1024 / 1024:F2} mbps";
             
        case WidgetType.Memory:
            var memoryStats = sensors.GetMemoryStats();
            return widget.MemorySensorTarget switch
            {
                MemorySensorTarget.UsedPercentage => $"{memoryStats.UsedPercentage:F2}" +
                                                     (widget.DisplayType == WidgetDisplayType.Readout ? "%" : ""),
                MemorySensorTarget.Total => $"{memoryStats.Total / 1024.0 / 1024.0:F0} GiB",
                MemorySensorTarget.Free => $"{memoryStats.Available / 1024.0 / 1024.0:F0} GiB",
                _ => "" // Default means we've got an unsupported target which will fail to pass JSON conversion anyway
            };

        case WidgetType.Sensor:
            if (widget.DeviceName is null || widget.SensorName is null || widget.FieldName is null)
                throw new Exception("Invalid sensor details in config");
            
            return sensors.GetSensorValueOrDefault(widget.DeviceName, widget.SensorName, widget.FieldName).ToString("F0");
        
        case WidgetType.Disk:
            if (widget.DeviceName is null)
                throw new Exception("Invalid sensor details in config");
            
            return $"{sensors.GetDiskStats(widget.DeviceName).Available / 1024.0 / 1024.0 / 1024.0:F2} GiB";
        
        default:
            return "";
    }
}
