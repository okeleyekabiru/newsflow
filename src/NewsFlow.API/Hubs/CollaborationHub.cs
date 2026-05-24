using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NewsFlow.API.Hubs;

[Authorize]
public class CollaborationHub : Hub
{
    public async Task JoinArticle(string articleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"article:{articleId}");
        await Clients.OthersInGroup($"article:{articleId}")
            .SendAsync("UserJoined", new
            {
                ConnectionId = Context.ConnectionId,
                UserId = Context.UserIdentifier,
                JoinedAt = DateTime.UtcNow
            });
    }

    public async Task LeaveArticle(string articleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"article:{articleId}");
        await Clients.OthersInGroup($"article:{articleId}")
            .SendAsync("UserLeft", Context.UserIdentifier);
    }

    public async Task SendCursorPosition(string articleId, int line, int column)
    {
        await Clients.OthersInGroup($"article:{articleId}")
            .SendAsync("CursorMoved", new
            {
                UserId = Context.UserIdentifier,
                Line = line,
                Column = column
            });
    }

    public async Task JoinReviewQueue()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "review-queue");
    }

    public async Task LeaveReviewQueue()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "review-queue");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}

public interface ICollaborationClient
{
    Task NewFlagAlert(object flaggedPost);
    Task UserJoined(object user);
    Task UserLeft(string userId);
    Task CursorMoved(object cursor);
    Task UserDisconnected(string userId);
}
