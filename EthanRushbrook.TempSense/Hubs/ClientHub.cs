using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EthanRushbrook.TempSense.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EthanRushbrook.TempSense.Hubs;

public class ClientHub(ILogger<ClientHub> logger) : Hub
{
    // (ConnectionID => PageIDs)
    public static readonly ConcurrentDictionary<string, List<string>> ClientIdToPageId = new();
    
    public override async Task OnConnectedAsync()
    {
        if (ServerInteropBridge.Instance is null)
        {
            logger.LogInformation("UI is not ready yet, denying incoming connection");
            
            Context.Abort(); // Give up, the client isn't ready yet
            return;
        }
        
        await base.OnConnectedAsync();

        ClientIdToPageId[Context.ConnectionId] = [];

        await Clients.Caller.SendAsync("RequestPageDefinition");
    }

    public async Task ReceivePageDefinition(PageDefinition pageDefinition)
    {
        if (ServerInteropBridge.Instance is null)
            return;

        if (ClientIdToPageId.Values.Any(x => x.Contains(pageDefinition.PageName)))
        {
            // TODO This should kick the original instead and replace with the new
            logger.LogWarning("Received a page that already is registered. Booting connection.");
            
            Context.Abort();

            return;
        }

        ClientIdToPageId[Context.ConnectionId].Add(pageDefinition.PageName);
        
        ServerInteropBridge.Instance.InitializePage(pageDefinition);
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

        ClientIdToPageId.Remove(Context.ConnectionId, out var pages);
        
        logger.LogInformation("Removing pages {PageId}", pages is not null ? string.Join(", ", pages) : "(none)");
        
        if (ServerInteropBridge.Instance is not null && pages is not null)
            foreach (var page in pages)
                ServerInteropBridge.Instance.RemovePage(page);
    }
}
