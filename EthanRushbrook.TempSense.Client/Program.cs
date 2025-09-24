using System.Diagnostics;
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


// Brief validation on pages and widgets
if (config.Pages is null || config.Pages.Any(x => x.Widgets is null))
    throw new Exception("Invalid page configuration");

var pages = config.Pages.Select(page => (Page: page, Widgets: page.Widgets!.Select(x => (Widget: new WidgetDefinition
{
    Id = Guid.NewGuid(),
    Header = x.DisplayType == WidgetDisplayType.Round ? GetWidgetValue(x) + x.Unit : x.Header,
    DisplayType = x.DisplayType ?? throw new Exception("Invalid display type in config"),
    Caption = x.DisplayName ?? "UNKNOWN",
    InitialValue = GetWidgetValue(x)
}, Definition: x)).ToList())).ToList();

if (pages.Any(x => x.Widgets.Any(w => w.Definition.Action is not null)))
{
    Console.WriteLine(Environment.NewLine + Environment.NewLine + "=============================================");
    Console.WriteLine("=========== YOU ARE USING ACTIONS ===========");
    Console.WriteLine("Actions execute your set code over the network. Use common sense.");
    Console.WriteLine("You are responsible for your network security.");
    Console.WriteLine("=============================================");
    Console.WriteLine("=============================================" + Environment.NewLine + Environment.NewLine);
}


var connection = new HubConnectionBuilder()
    .WithUrl(config.ServerEndpoint ?? throw new Exception("No server endpoint set in config"))
    .Build();

// Set up widget page on the server
connection.On("RequestPageDefinition", async () =>
{
    foreach (var page in pages)
    {
        await connection.SendAsync("ReceivePageDefinition", new PageDefinition
        {
            PageName = page.Page.PageName ?? throw new Exception("Missing page name"),
            Widgets = page.Widgets.Select(x => x.Widget).ToList(),
            Layout = page.Page.WidgetLayout ?? throw new Exception("Missing page layout"),
            RowDefinitions = page.Page.RowDefinitions ?? throw new Exception("Missing page row definitions"),
            ColumnDefinitions = page.Page.ColumnDefinitions ?? throw new Exception("Missing page column definitions"),
        });
    }
});

// Actions
connection.On("PerformAction", (string pageId, Guid widgetId) =>
{
    var widget = pages
        .Find(x => x.Page.PageName == pageId)
        .Widgets
        .Find(x => x.Widget.Id == widgetId);

    // Create the process and immediately dispose of it so the OS has control
    Process.Start(new ProcessStartInfo
    {
        FileName = widget.Definition.Action,
        Arguments = widget.Definition.ActionArgs ?? "",
        CreateNoWindow = true,
        UseShellExecute = false
    })?.Dispose();
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
    var pageUpdate = pages.Select(page => (page.Page.PageName, DataPoints: page.Widgets.Select(x => new DataPoint
    {
        WidgetId = x.Widget.Id,
        NewValue = GetWidgetValue(x.Definition),
        NewHeader = x.Definition.Type == WidgetType.Sensor
            ? GetWidgetValue(x.Definition) + x.Definition.Unit
            : x.Widget.Header
    }).ToList())).ToList();

    foreach (var update in pageUpdate)
        if (connection.State == HubConnectionState.Connected)
            await connection.SendAsync("ReceiveDataframe", new Dataframe
            {
                PageId = update.PageName ?? throw new Exception("No page name is set in config"),
                DataPoints = update.DataPoints
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
