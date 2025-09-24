using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using EthanRushbrook.TempSense.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace EthanRushbrook.TempSense.Components.PageLayouts;

public class PageLayout(string pageId) : UserControl
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
    
    public async Task OnAction(Guid widgetId)
    {
        if (TempSenseApp.ServiceProvider is null)
            throw new Exception("No registered service provider");
        
        await using var serviceScope = TempSenseApp.ServiceProvider.CreateAsyncScope();

        var hub = serviceScope.ServiceProvider.GetRequiredService<IHubContext<ClientHub>>();

        // The time complexity of this sucks ass, but we will never have enough pages/widgets for this to be too slow.
        // I do realize that this is extremely inefficient, but it will never matter. The UI would slow to a crawl
        // before this became the problem.
        await hub
            .Clients
            .Client(ClientHub
                .ClientIdToPageId
                .FirstOrDefault(x => x.Value.Contains(pageId))
                .Key)
            .SendAsync("PerformAction", pageId, widgetId, cancellationToken: CancellationToken.None);
    }
}
