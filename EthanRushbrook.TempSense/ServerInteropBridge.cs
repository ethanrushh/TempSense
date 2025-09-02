using System.Collections.Generic;
using Avalonia.Threading;
using EthanRushbrook.TempSense.Contracts;

namespace EthanRushbrook.TempSense;

public class ServerInteropBridge
{
    public static ServerInteropBridge? Instance;
    
    
    private readonly MainWindow _window;
    public ServerInteropBridge(MainWindow window)
    {
        _window = window;
    }

    public void InitializePage(string pageId, List<WidgetDefinition> widgets, WidgetLayout layout)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _window.InitializePage(pageId, widgets, layout);
        });
    }

    public void ApplyDataframe(Dataframe dataframe)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _window.ApplyDataframe(dataframe);
        });
    }

    public void RemovePage(string pageId)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _window.RemovePage(pageId);
        });
    }
}
