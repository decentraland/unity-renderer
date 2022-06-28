using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatNotificationController : IHUD
{
    IChatController chatController;

    public ChatNotificationController(IChatController chatController)
    {
        this.chatController = chatController;
    }

    public void Initialize()
    { 
        
    }

    public void SetVisibility(bool visible)
    {
    }

    public void Dispose()
    {
    }
}
