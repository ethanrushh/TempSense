using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EthanRushbrook.TempSense.Components.Widgets;

public partial class WidgetTemplate : UserControl
{
    public static readonly StyledProperty<object?> WidgetProperty =
        AvaloniaProperty.Register<WidgetTemplate, object?>(nameof(Widget));

    public object? Widget
    {
        get => GetValue(WidgetProperty);
        set => SetValue(WidgetProperty, value);
    }
    
    public WidgetTemplate()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Widget is null or not Control)
            return;

        CentrePanel.Children.Clear();
        CentrePanel.Children.Add((Control)Widget);
    }
}
