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

            OnWidgetsChanged(this, null!);
        });
        this.GetObservable(RowsProperty).Subscribe(rows =>
        {
            LayoutGrid.RowDefinitions.Clear();

            if (rows is null)
                return;
            
            foreach (var row in rows)
                LayoutGrid.RowDefinitions.Add(row);

            OnWidgetsChanged(this, null!);
        });
    }

    private void OnWidgetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (LayoutGrid == null) 
            return;

        LayoutGrid.Children.Clear();

        var columns = LayoutGrid.ColumnDefinitions.Count;
        var rows = LayoutGrid.RowDefinitions.Count;

        // Avoid a div by zero error
        if (columns == 0 || rows == 0)
            return;

        for (var i = 0; i < Widgets.Count; i++)
        {
            var child = Widgets[i].Control;

            var row = i / columns;
            var col = i % columns;

            if (row >= rows)
                break;

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

        OnWidgetsChanged(this, null!);
    }
}
