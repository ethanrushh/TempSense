using Avalonia;
using Avalonia.Controls;

namespace EthanRushbrook.TempSense.Components.Widgets;

public partial class CircleWidget : UserControl
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<CircleWidget, string?>(nameof(Value));
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<CircleWidget, string?>(nameof(Caption));
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }
    
    public CircleWidget()
    {
        InitializeComponent();
    }
}
