using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using EthanRushbrook.TempSense.Components.PageLayouts;

namespace EthanRushbrook.TempSense.Components.Widgets;

public abstract class WidgetBase(Guid widgetId) : UserControl
{
    private EventHandler<TappedEventArgs>? _tapHandler;
    
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _tapHandler ??= async (_, _) =>
        {
            var page = this.FindAncestorOfType<PageLayout>();
            if (page == null)
                throw new Exception("Widgets must be inside a PageLayout");
            await page.OnAction(widgetId);
        };
        
        // Get the card
        var card = this.FindAncestorOfType<WidgetTemplate>();

        if (card == null)
            throw new Exception("Widgets must be inside a WidgetTemplate");

        card.Tapped += _tapHandler;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Get the card
        var card = this.FindAncestorOfType<WidgetTemplate>();

        if (card == null)
            throw new Exception("Widgets must be inside a WidgetTemplate");

        card.Tapped -= _tapHandler;
        
        base.OnDetachedFromVisualTree(e);
    }
}
