using System;
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
        
        this.GetObservable(HeaderProperty).Subscribe(value =>
        {
            TitleViewbox.IsVisible = !string.IsNullOrEmpty(value);
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        TitleViewbox.IsVisible = !string.IsNullOrEmpty(Header);
    }
}
