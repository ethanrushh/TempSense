using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EthanRushbrook.TempSense.Contracts;

namespace EthanRushbrook.TempSense.Client;

public class ConfigModel
{
    [JsonRequired]
    [JsonPropertyName("server_endpoint")]
    public string? ServerEndpoint { get; set; }
    
    [JsonRequired]
    [JsonPropertyName("page_id")]
    public string? PageId { get; set; }

    [JsonRequired]
    [JsonPropertyName("widget_layout")]
    public string? WidgetLayout { get; set; }
    
    [JsonRequired]
    [JsonPropertyName("widgets")]
    public List<ConfigWidgetDefinition>? Widgets { get; set; }
}

public class ConfigWidgetDefinition
{
    [JsonRequired]
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    
    [JsonRequired]
    [JsonPropertyName("type")]
    public WidgetType? Type { get; set; }
    
    [JsonRequired]
    [JsonPropertyName("widget_display_type")]
    public WidgetDisplayType? DisplayType { get; set; }
    
    [JsonPropertyName("header")]
    public string? Header { get; set; }
    
    [JsonPropertyName("device_name")]
    public string? DeviceName { get; set; }
    
    [JsonPropertyName("sensor_name")]
    public string? SensorName { get; set; }
    
    [JsonPropertyName("field_name")]
    public string? FieldName { get; set; }
    
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
    
    [JsonPropertyName("direction")]
    public NetworkDirection? Direction { get; set; }
    
    [JsonPropertyName("memory_sensor_target")]
    public MemorySensorTarget? MemorySensorTarget { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidgetType
{
    Sensor, Memory, Network
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetworkDirection
{
    Up, Down
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemorySensorTarget
{
    Free, Total, UsedPercentage
}

public static class ConfigModelExtensions
{
    public static void ValidateOrThrow(this ConfigModel config)
    {
        if (config.ServerEndpoint is null)
            throw new ValidationException($"{nameof(config.ServerEndpoint)} is required");
    }
}
