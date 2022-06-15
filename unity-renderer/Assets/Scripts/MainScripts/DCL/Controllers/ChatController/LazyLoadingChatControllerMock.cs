using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChatController))]
public class LazyLoadingChatControllerMock : IChatController
{
    private ChatController controller;

    public LazyLoadingChatControllerMock(ChatController controller)
    {
        this.controller = controller;
    }

    public event Action<ChatMessage> OnAddMessage
    {
        add => controller.OnAddMessage += value;
        remove => controller.OnAddMessage -= value;
    }

    public List<ChatMessage> GetAllocatedEntries() => controller.GetAllocatedEntries();

    public void Send(ChatMessage message) => controller.Send(message);
    
    public void MarkMessagesAsSeen(string userId) { controller.MarkMessagesAsSeen(userId); }

    public void GetPrivateMessages(string userId, int limit, long fromTimestamp)
    {
        // TODO: Delay...
        // TODO: controller.AddMessageToChatWindow(fake message)
    }
}