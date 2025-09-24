using System;
using Avalonia;

namespace EthanRushbrook.TempSense.Components.Widgets;

public partial class FluidWidget : WidgetBase
{
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<FluidWidget, string?>(nameof(Caption));
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }
    
    public FluidWidget(Guid widgetId) : base(widgetId)
    {
        InitializeComponent();
    }
}
