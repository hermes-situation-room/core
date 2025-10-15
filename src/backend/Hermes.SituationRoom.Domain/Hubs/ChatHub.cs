namespace Hermes.SituationRoom.Domain.Hubs;

using Microsoft.AspNetCore.SignalR;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.RegularExpressions;

[Authorize]
public class ChatHub(IChatService chatService, IUserChatReadStatusService userChatStatusSearvice) : Hub
{

    public async Task JoinMessaging()
    {
        var userId = GetUserIdFromContext();
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
    }

    public async Task LeaveMessaging()
    {
        var userId = GetUserIdFromContext();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
    }
    public async Task JoinChat(Guid chatId)
    {
        if (chatId == Guid.Empty)
        {
            throw new HubException("No Guid provided.");
        }

        var userId = GetUserIdFromContext();

        try
        {
            var chatBo = await chatService.GetChatAsync(chatId);
            if (chatBo.User1Uid != userId && chatBo.User2Uid != userId)
            {
                throw new HubException($"User: {userId} is not part of Chat: {chatId}.");
            }
            await userChatStatusSearvice.UpdateReadStatusAsync(userId, chatBo.Uid);
        }
        catch (KeyNotFoundException)
        {
            throw new HubException($"Chat with Id: {chatId} not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task LeaveChat(Guid chatId)
    {
        var userId = GetUserIdFromContext();
        await userChatStatusSearvice.UpdateReadStatusAsync(userId, chatId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task UpdateReadChat(Guid chatId)
    {
        var userId = GetUserIdFromContext();
        await userChatStatusSearvice.UpdateReadStatusAsync(userId, chatId);
    }

    private Guid GetUserIdFromContext()
    {
        var cookieUserId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(cookieUserId, out var userId))
            throw new HubException("UserId from Cookie not valid");
        return userId;
    }
}
