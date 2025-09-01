using Avalonia;
using Avalonia.Controls;

namespace EthanRushbrook.TempSense.Components.Widgets;

public abstract class WidgetBase : UserControl
{
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<WidgetBase, object?>(nameof(Value));
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<WidgetBase, string?>(nameof(Header));
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
