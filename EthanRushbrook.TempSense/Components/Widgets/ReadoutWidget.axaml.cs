using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Reactive;

namespace EthanRushbrook.TempSense.Components.Widgets;

public partial class ReadoutWidget : WidgetBase
{
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        var initialTitleHeight = TitleViewbox.MaxHeight;
        TitleViewbox.MaxHeight = Header is null ? 0 : initialTitleHeight;

        HeaderProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string?>>(value =>
        {
            TitleViewbox.MaxHeight = value.NewValue.GetValueOrDefault() is null ? 0 : initialTitleHeight;
        }));
    }
}
