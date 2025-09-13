using System.Text.Json.Serialization;

namespace EthanRushbrook.TempSense.Contracts;

public class WidgetDefinition
{
    public Guid Id { get; set; }
    
    public WidgetDisplayType DisplayType { get; set; }
    
    public string Caption { get; set; }
    
    public string InitialValue { get; set; }
    
    public string? Header { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidgetDisplayType
{
    Round, Fluid, Readout
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidgetLayout
{
    Grid
}
