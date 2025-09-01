using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Reactive;

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
    
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ReadoutWidget, string?>(nameof(Title));
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ReadoutWidget()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        var initialTitleHeight = TitleViewbox.MaxHeight;
        TitleViewbox.MaxHeight = Title is null ? 0 : initialTitleHeight;

        TitleProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string?>>(value =>
        {
            TitleViewbox.MaxHeight = value.NewValue.GetValueOrDefault() is null ? 0 : initialTitleHeight;
        }));
    }
}
