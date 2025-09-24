using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using EthanRushbrook.TempSense.Components.PageLayouts;

namespace EthanRushbrook.TempSense.Components.Widgets;

public abstract class WidgetBase(Guid widgetId) : UserControl
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Get the card
        var card = this.FindAncestorOfType<WidgetTemplate>();

        if (card == null)
            throw new Exception("Widgets must be inside a WidgetTemplate");

        card.Tapped += async (_, _) =>
        {
            var page = this.FindAncestorOfType<PageLayout>();
            
            if (page == null)
                throw new Exception("Widgets must be inside a PageLayout");
            
            await page.OnAction(widgetId);
        };
        
        base.OnAttachedToVisualTree(e);
    }
}
