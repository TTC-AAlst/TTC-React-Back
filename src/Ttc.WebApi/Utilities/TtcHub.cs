using Microsoft.AspNetCore.SignalR;
using Ttc.Model.Core;

namespace Ttc.WebApi.Utilities;

public enum Entities
{
    Player,
    Match,
    Team,
    Club,
    Config,
}

public class TtcHub : Hub
{
    private readonly TtcLogger _logger;

    public TtcHub(TtcLogger logger)
    {
        _logger = logger;
    }

    public async Task BroadcastReload(Entities entityType, int id)
    {
        _logger.Information($"BroadcastReload {entityType} for {id} with ClientsNull={(Clients == null).ToString()} and Context={Context?.ConnectionId}");

        if (Clients != null && Context != null)
        {
            await Clients.AllExcept(Context.ConnectionId).SendAsync("BroadcastReload", entityType.ToString(), id);
        }
        else
        {
            _logger.Information("Not broadcasting :(");
        }
    }

    //public async Task SendMessage(string user, string message)
    //{
    //    await Clients.All.SendAsync("ReceiveMessage", user, message);
    //    //await Clients.Group("Clubbers").SendAsync("ReceiveMessage", $"Logged-in message: {message}");
    //    //await Clients.Group("Visitors").SendAsync("ReceiveMessage", $"Guest message: {message}");
    //}

    //public override async Task OnConnectedAsync()
    //{
    //    // Context.User is only authenticated when adding [Authorize] to the Hub
    //    // Otherwise we need to validate the token manually:
    //    // string jwt = Context.GetHttpContext().Request.Query["access_token"];

    //    bool isLoggedIn = Context.User?.Identity?.IsAuthenticated ?? false;
    //    if (isLoggedIn)
    //    {
    //        await Groups.AddToGroupAsync(Context.ConnectionId, "Clubbers");
    //    }
    //    else
    //    {
    //        await Groups.AddToGroupAsync(Context.ConnectionId, "Visitors");
    //    }

    //    await base.OnConnectedAsync();
    //}

    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Clubbers");
    //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Visitors");
    //    await base.OnDisconnectedAsync(exception);
    //}
}
