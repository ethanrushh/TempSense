using System;
using System.IO;
using System.Linq;
using SukiUI.Controls;
using Avalonia.Controls;
using Avalonia.Collections;
using Avalonia.Interactivity;
using System.Collections.Generic;
using EthanRushbrook.TempSense.Contracts;
using EthanRushbrook.TempSense.SystemInterop;
using EthanRushbrook.TempSense.Components.Widgets;
using EthanRushbrook.TempSense.Components.PageLayouts;

namespace EthanRushbrook.TempSense;

public partial class MainWindow : SukiWindow
{
    /// <summary>
    /// (pageId => (widgetId => widget))
    /// </summary>
    private Dictionary<string, (TabItem TabControl, Dictionary<Guid, WidgetTemplate> Widgets)> _pages = new();
    
    public MainWindow()
    {
        InitializeComponent();

        ServerInteropBridge.Instance = new ServerInteropBridge(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        InitializeWindowState();
        DetectLocalIp();
    }

    // The server should prevent double-page loading and its okay to trust that
    public void InitializePage(PageDefinition pageDefinition)
    {
        // Build widgets
        var widgetControls = pageDefinition.Widgets
            .Select(x => (x.Id, WidgetControl: WidgetConstructor.ConstructWidget(x)))
            .ToArray();

        // Build page, attach widgets
        PageLayout pageLayout = pageDefinition.Layout switch
        {
            WidgetLayout.Minis => new MinisPageLayout(),
            WidgetLayout.MinisWithFooter => new MinisWithFooterPageLayout(),
            _ => throw new ArgumentException("Layout is not supported or valid", nameof(pageDefinition))
        };
        pageLayout.Widgets = new AvaloniaList<(Guid, Control)>(widgetControls.Select(x => (x.Id, x.WidgetControl as Control)));
        pageLayout.Rows = RowDefinitions.Parse(pageDefinition.RowDefinitions);
        pageLayout.Columns = ColumnDefinitions.Parse(pageDefinition.ColumnDefinitions);
        
        // Build tab
        var tab = new TabItem
        {
            Header = new TextBlock { Text = pageDefinition.PageName },
            Content = pageLayout
        };

        // Attach tab, register the whole chain
        TabControl.Items.Add(tab);

        _pages[pageDefinition.PageName] = (tab, widgetControls.ToDictionary(x => x.Id, x => x.WidgetControl));
    }

    public void RemovePage(string pageId)
    {
        _pages.Remove(pageId, out var value);

        TabControl.Items.Remove(value.TabControl);
    }

    // TODO: Instead of changing the property each time (could change >1 time per frame...), it should update a local state and each UI frame each widget should update itself
    public void ApplyDataframe(Dataframe dataframe)
    {
        foreach (var dataPoint in dataframe.DataPoints)
        {
            if (_pages.TryGetValue(dataframe.PageId, out var page))
            {
                if (page.Widgets.TryGetValue(dataPoint.WidgetId, out var widgetTemplate))
                {
                    // This is horrible wtf
                    if (widgetTemplate.Widget is not WidgetBase widget)
                        continue;
                    
                    widget.Value = dataPoint.NewValue;

                    // Update header, if requested
                    if (dataPoint.NewHeader is not null)
                        widget.Header = dataPoint.NewHeader;
                }
            }
        }
    }

    private void InitializeWindowState()
    {
        if (!File.Exists("/proc/cpuinfo"))
            return;

        var lines = File.ReadAllLines("/proc/cpuinfo");

        // Look for Hardware, Model, or Revision fields
        var hardware = lines.FirstOrDefault(l => l.StartsWith("Hardware"));
        var model = lines.FirstOrDefault(l => l.StartsWith("Model"));

        // Raspberry Pi 5 has BCM2712 SoC
        if ((hardware?.Contains("BCM2712") ?? false) ||
            (model?.Contains("Raspberry Pi 5") ?? false))
        {
            Console.WriteLine("CPU is a BCM2712: Setting to fullscreen (Pi 5 detected)");
            
            WindowState = WindowState.FullScreen;
        }
        else
            Console.WriteLine("Pi 5 not detected, starting in dev mode");
    }

    private void DetectLocalIp()
    {
        var localIp = LocalNetworkInterop.GetLocalIPv4();
        
        LocalIpHeader.Header = localIp ?? "No IPv4 detected";
    }
}
