using System.Text.Json;
using EthanRushbrook.TempSense.Client;
using EthanRushbrook.TempSense.Client.Linux;


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

while (true)
{
    sensors.Tick();

    Console.Clear();
    // Console.WriteLine(
    //     config.Widgets.First().DisplayName + ": " + sensors.GetSensorValueOrDefault(
    //         config.Widgets.First().DeviceName, config.Widgets.First().SensorName,
    //         config.Widgets.First().FieldName) + config.Widgets.First().Unit
    // );

    foreach (var widget in config.Widgets)
    {
        switch (widget.Type)
        {
            case WidgetType.Network:
                
                var speed = widget.Direction == NetworkDirection.Down
                    ? sensors.GetNetworkStats().DownloadSpeed
                    : sensors.GetNetworkStats().UploadSpeed;
                
                Console.WriteLine($"{widget.DisplayName}: {speed * 8 / 1024 / 1024:F2} mbps");
                break;
            
            case WidgetType.Memory:
                Console.WriteLine($"{widget.DisplayName}: {sensors.GetMemoryStats().UsedPercentage:F2}");
                break;
            
            case WidgetType.Sensor:
                Console.WriteLine($"{widget.DisplayName}: {sensors.GetSensorValueOrDefault(widget.DeviceName, widget.SensorName, widget.FieldName)}{widget.Unit}");
                break;
        }
    }
    
    await Task.Delay(500);
}

