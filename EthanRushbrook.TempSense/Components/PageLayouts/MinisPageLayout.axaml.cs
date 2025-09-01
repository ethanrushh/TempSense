using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Specialized;

namespace EthanRushbrook.TempSense.Components.PageLayouts;

public partial class MinisPageLayout : UserControl
{
    public static readonly StyledProperty<AvaloniaList<(Guid Id, Control Control)>> WidgetsProperty =
        AvaloniaProperty.Register<MinisPageLayout, AvaloniaList<(Guid, Control)>>(nameof(Widgets));

    public AvaloniaList<(Guid Id, Control Control)> Widgets
    {
        get => GetValue(WidgetsProperty);
        set => SetValue(WidgetsProperty, value);
    }

    public MinisPageLayout()
    {
        InitializeComponent();

        // Ensure list is never null
        Widgets = [];
        Widgets.CollectionChanged += OnWidgetsChanged;
    }

    private void OnWidgetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (LayoutGrid == null) 
            return;

        LayoutGrid.Children.Clear();

        for (var i = 0; i < Widgets.Count; i++)
        {
            var child = Widgets[i].Control;

            // 2 columns, 3 rows
            var row = i / 2;    // 0,0,1,1,2,2
            var col = i % 2;    // 0,1,0,1,0,1

            Grid.SetRow(child, row);
            Grid.SetColumn(child, col);

            LayoutGrid.Children.Add(child);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        if (LayoutGrid == null) 
            return;

        LayoutGrid.Children.Clear();

        for (var i = 0; i < Widgets.Count; i++)
        {
            var child = Widgets[i].Control;

            var row = i / 2;
            var col = i % 2;

            Grid.SetRow(child, row);
            Grid.SetColumn(child, col);

            LayoutGrid.Children.Add(child);
        }
    }
}
