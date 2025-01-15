using Microsoft.AspNetCore.SignalR;

namespace Ttc.WebApi.Utilities;

public enum Entities
{
    Player,
    Match,
    Team,
    Club,
    Config,
}

public interface ITtcHub
{
    Task BroadcastReload(Entities entityType, int id);
}

public class TtcHub : Hub<ITtcHub>
{
    //public async Task BroadcastReload(Entities entityType, int id)
    //{
    //    if (Clients != null && Context != null)
    //    {
    //        await Clients.All.SendAsync("BroadcastReload", entityType.ToString(), id);
    //    }
    //    else
    //    {
    //        _logger.Error($"Not broadcasting {entityType} for {id} :(");
    //    }
    //}

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
