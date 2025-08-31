using Avalonia;
using Avalonia.Controls;

namespace EthanRushbrook.TempSense.Components.Widgets;

public partial class ReadoutWidget : UserControl
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<ReadoutWidget, string?>(nameof(Value));
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<ReadoutWidget, string?>(nameof(Caption));
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }
    
    public ReadoutWidget()
    {
        InitializeComponent();
    }
}
