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

    public void InitializePage(PageDefinition pageDefinition)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _window.InitializePage(pageDefinition);
        });
    }

    public void ApplyDataframe(Dataframe dataframe)
    {
        Dispatcher.UIThread.Post(() =>
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
