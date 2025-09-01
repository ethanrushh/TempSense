using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using SukiUI;
using SukiUI.Enums;

namespace EthanRushbrook.TempSense.Components.Widgets;

public partial class CircleWidget : WidgetBase
{
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<CircleWidget, string?>(nameof(Caption));
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }
    
    public static readonly StyledProperty<SukiColor?> ColorProperty =
        AvaloniaProperty.Register<CircleWidget, SukiColor?>(nameof(Color));
    public SukiColor? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
    
    public CircleWidget()
    {
        InitializeComponent();
    }
}
