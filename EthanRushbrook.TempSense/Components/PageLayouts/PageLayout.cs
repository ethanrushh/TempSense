using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;

namespace EthanRushbrook.TempSense.Components.PageLayouts;

public class PageLayout : UserControl
{
    public static readonly StyledProperty<AvaloniaList<(Guid Id, Control Control)>> WidgetsProperty =
        AvaloniaProperty.Register<PageLayout, AvaloniaList<(Guid, Control)>>(nameof(Widgets));

    public AvaloniaList<(Guid Id, Control Control)> Widgets
    {
        get => GetValue(WidgetsProperty);
        set => SetValue(WidgetsProperty, value);
    }
    
    public static readonly StyledProperty<RowDefinitions> RowsProperty =
        AvaloniaProperty.Register<PageLayout, RowDefinitions>(nameof(Rows));

    public RowDefinitions Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }
    
    public static readonly StyledProperty<ColumnDefinitions> ColumnsProperty =
        AvaloniaProperty.Register<PageLayout, ColumnDefinitions>(nameof(Columns));

    public ColumnDefinitions Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }
}
