using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EthanRushbrook.TempSense.Components.PageLayouts;
using EthanRushbrook.TempSense.Components.Widgets;
using EthanRushbrook.TempSense.Contracts;
using EthanRushbrook.TempSense.SystemInterop;
using SukiUI.Controls;

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

    // The server should prevent double-page loading
    public void InitializePage(string pageId, List<WidgetDefinition> widgets)
    {
        var widgetControls = widgets
            .Select(x => (x.Id, WidgetControl: WidgetConstructor.ConstructWidget(x)))
            .ToArray();
        
        var tab = new TabItem
        {
            Header = new TextBlock { Text = pageId },
            Content = new MinisPageLayout
            {
                [MinisPageLayout.WidgetsProperty] = new AvaloniaList<(Guid, Control)>(widgetControls.Select(x => (x.Id, x.WidgetControl as Control)))
            }
        };

        TabControl.Items.Add(tab);

        _pages[pageId] = (tab, widgetControls.ToDictionary(x => x.Id, x => x.WidgetControl));
    }

    public void RemovePage(string pageId)
    {
        _pages.Remove(pageId, out var value);

        TabControl.Items.Remove(value.TabControl);
    }

    public void ApplyDataframe(Dataframe dataframe)
    {
        foreach (var dataPoint in dataframe.DataPoints)
        {
            if (_pages.TryGetValue(dataframe.PageId, out var page))
            {
                if (page.Widgets.TryGetValue(dataPoint.WidgetId, out var widgetTemplate))
                {
                    // This is horrible wtf
                    if (widgetTemplate[WidgetTemplate.WidgetProperty] is not WidgetBase widget)
                        continue;
                    
                    widget[WidgetBase.ValueProperty] = dataPoint.NewValue;

                    // Update header, if requested
                    if (dataPoint.NewHeader is not null)
                        widget[WidgetBase.HeaderProperty] = dataPoint.NewHeader;
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
