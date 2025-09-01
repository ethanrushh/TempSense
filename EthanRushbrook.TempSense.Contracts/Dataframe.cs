namespace EthanRushbrook.TempSense.Contracts;

public class Dataframe
{
    public string PageId { get; set; }
    public IList<DataPoint> DataPoints { get; set; }
}

public class DataPoint
{
    public Guid WidgetId { get; set; }

    public string NewValue { get; set; } = string.Empty;
    public string? NewHeader { get; set; }
}
