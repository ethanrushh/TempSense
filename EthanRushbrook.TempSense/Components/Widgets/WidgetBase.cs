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
    private WidgetTemplate? _card;
    
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

        _card = this.FindAncestorOfType<WidgetTemplate>()
                ?? throw new Exception("Widgets must be inside a WidgetTemplate");

        _card.Tapped += _tapHandler;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_card != null && _tapHandler != null)
        {
            _card.Tapped -= _tapHandler;
            _card = null; // clear reference to avoid leaks
        }
        
        base.OnDetachedFromVisualTree(e);
    }
}
