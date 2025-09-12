namespace EthanRushbrook.TempSense.Contracts;

public class PageDefinition
{
    public string PageName { get; set; }
    public List<WidgetDefinition> Widgets { get; set; }
    public WidgetLayout Layout { get; set; }
    public string RowDefinitions { get; set; }
    public string ColumnDefinitions { get; set; }
}
