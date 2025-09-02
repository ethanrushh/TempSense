using System.Collections.Concurrent;
using EthanRushbrook.TempSense.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace EthanRushbrook.TempSense.Server.Hubs;

public class ClientHub(ILogger<ClientHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, string> ClientIdToPageId = new();
    
    public override async Task OnConnectedAsync()
    {
        if (ServerInteropBridge.Instance is null)
        {
            logger.LogInformation("UI is not ready yet, denying incoming connection");
            
            Context.Abort(); // Give up, the client isn't ready yet
            return;
        }
        
        await base.OnConnectedAsync();

        await Clients.Caller.SendAsync("HelloFromServer");
        await Clients.Caller.SendAsync("RequestPageDefinition");
    }

    public async Task ReceivePageDefinition(string pageId, List<WidgetDefinition> widgets, WidgetLayout layout)
    {
        if (ServerInteropBridge.Instance is null)
            return;

        if (ClientIdToPageId.Values.Contains(pageId))
        {
            // TODO This should kick the original instead and replace with the new
            logger.LogWarning("Received a page that already is registered. Booting connection.");
            
            Context.Abort();

            return;
        }

        ClientIdToPageId[Context.ConnectionId] = pageId;
        
        ServerInteropBridge.Instance.InitializePage(pageId, widgets, layout);
    }

    public async Task ReceiveDataframe(Dataframe dataframe)
    {
        if (ServerInteropBridge.Instance is null)
            return;
        
        ServerInteropBridge.Instance.ApplyDataframe(dataframe);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);

        ClientIdToPageId.Remove(Context.ConnectionId, out var pageId);
        
        logger.LogInformation("Removing page {PageId}", pageId ?? "(none)");
        
        if (ServerInteropBridge.Instance is not null && pageId is not null)
            ServerInteropBridge.Instance.RemovePage(pageId);
    }
}
