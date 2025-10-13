namespace Hermes.SituationRoom.Domain.Hubs;

using Microsoft.AspNetCore.SignalR;
using Interfaces;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ChatHub(IChatService chatService) : Hub
{
    public async Task JoinChat(Guid userId, Guid chatId)
    {    
        if (chatId == Guid.Empty)
        {
            throw new HubException("No Guid provided.");
        }
        
        try
        {
            var chatBo = await chatService.GetChatAsync(chatId);
            if (chatBo.User1Uid != userId && chatBo.User2Uid != userId)
            {
                throw new HubException($"User: {userId} is not part of Chat: {chatId}.");
            }
        }
        catch (KeyNotFoundException)
        {
            throw new HubException($"Chat with Id: {chatId} not found.");
        }
    
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }
}
