using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;

namespace EthanRushbrook.TempSense.Components.PageLayouts;

public class PageLayout : UserControl
{
    public static readonly StyledProperty<AvaloniaList<(Guid Id, Control Control)>> WidgetsProperty =
        AvaloniaProperty.Register<MinisPageLayout, AvaloniaList<(Guid, Control)>>(nameof(Widgets));

    public AvaloniaList<(Guid Id, Control Control)> Widgets
    {
        get => GetValue(WidgetsProperty);
        set => SetValue(WidgetsProperty, value);
    }
}