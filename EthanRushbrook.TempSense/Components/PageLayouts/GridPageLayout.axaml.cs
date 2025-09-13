using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Specialized;

namespace EthanRushbrook.TempSense.Components.PageLayouts;

public partial class GridPageLayout : PageLayout
{
    public GridPageLayout()
    {
        InitializeComponent();

        // Ensure list is never null
        Widgets = [];
        Widgets.CollectionChanged += OnWidgetsChanged;
        
        this.GetObservable(ColumnsProperty).Subscribe(cols =>
        {
            LayoutGrid.ColumnDefinitions.Clear();

            if (cols is null)
                return;
            
            foreach (var col in cols)
                LayoutGrid.ColumnDefinitions.Add(col);
        });
        this.GetObservable(RowsProperty).Subscribe(rows =>
        {
            LayoutGrid.RowDefinitions.Clear();

            if (rows is null)
                return;
            
            foreach (var row in rows)
                LayoutGrid.RowDefinitions.Add(row);
        });
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
