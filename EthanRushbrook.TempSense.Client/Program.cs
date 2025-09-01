using System.Text.Json;
using EthanRushbrook.TempSense.Client;
using EthanRushbrook.TempSense.Client.Linux;
using EthanRushbrook.TempSense.Contracts;
using Microsoft.AspNetCore.SignalR.Client;

var configFile = File.OpenRead("config.json");

var jsonDocument = await JsonDocument.ParseAsync(configFile);

configFile.Close();
await configFile.DisposeAsync();

var config = jsonDocument.Deserialize<ConfigModel>();

if (config is null)
    throw new JsonException("Invalid config file");

config.ValidateOrThrow();

Console.WriteLine("DEBUG: Parsed config file");

var sensors = new LinuxSensors();
sensors.Tick();

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
    await connection.SendAsync("ReceivePageDefinition", config.PageId, widgets.Select(x => x.Widget));
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

// Main loop
while (true)
{
    // Update once per second
    sensors.Tick();
    
    if (connection.State == HubConnectionState.Connected)
        await connection.SendAsync("ReceiveDataframe", new Dataframe
        {
            PageId = config.PageId ?? throw new Exception("No page ID is set in config"),
            DataPoints = widgets.Select(x => new DataPoint
            {
                WidgetId = x.Widget.Id,
                NewValue = GetWidgetValue(x.Definition),
                NewHeader = x.Definition.Type == WidgetType.Sensor ? GetWidgetValue(x.Definition) + x.Definition.Unit : x.Widget.Header
            }).ToList()
        });
    
    await Task.Delay(100);
}




string GetWidgetValue(ConfigWidgetDefinition widget)
{
    switch (widget.Type)
    {
        case WidgetType.Network:
            var speed = widget.Direction == NetworkDirection.Down
                ? sensors.GetNetworkStats().DownloadSpeed
                : sensors.GetNetworkStats().UploadSpeed;
            return $"{speed * 8 / 1024 / 1024:F2} mbps";
             
        case WidgetType.Memory:
            return $"{sensors.GetMemoryStats().UsedPercentage:F2}";
             
        case WidgetType.Sensor:
            if (widget.DeviceName is null || widget.SensorName is null || widget.FieldName is null)
                throw new Exception("Invalid sensor details in config");
            
            return sensors.GetSensorValueOrDefault(widget.DeviceName, widget.SensorName, widget.FieldName)?.ToString() ?? "0";
        
        default:
            return "";
    }
}
