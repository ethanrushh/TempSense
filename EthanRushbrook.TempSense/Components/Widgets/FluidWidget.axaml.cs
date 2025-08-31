using Avalonia;
using Avalonia.Controls;

namespace EthanRushbrook.TempSense.Components.Widgets;

public partial class FluidWidget : UserControl
{
    public static readonly StyledProperty<double?> ValueProperty =
        AvaloniaProperty.Register<FluidWidget, double?>(nameof(Value));
    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<FluidWidget, string?>(nameof(Caption));
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }
    
    public FluidWidget()
    {
        InitializeComponent();
        
        // Set DataContext to self so bindings inside XAML are shorter
        DataContext = this;

        // ValueProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<double?>>(value =>
        // {
        //     Progress.Value = value.NewValue.Value ?? 0;
        // }));
    }

    // protected override void OnLoaded(RoutedEventArgs e)
    // {
    //     base.OnLoaded(e);
    //
    //     Progress.Value = Value ?? 0;
    // }
}
