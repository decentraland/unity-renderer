using System;
using System.Collections;
using DCL;
using DCL.Interface;
using UnityEngine;

public class TeleportController : ITeleportController
{

    public void Teleport(int x, int y)
    {
        WebInterface.GoTo(x, y);
    }

    public void JumpIn(int coordsX, int coordsY, string serverName, string layerName)
    {
        WebInterface.JumpIn(coordsX, coordsY, serverName, layerName);
    }

    public void GoToCrowd()
    {
        WebInterface.GoToCrowd();
    }

    public void JumpInWorld(string worldName)
    {
        WebInterface.SendChatMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.NONE,
            recipient = string.Empty,
            body = $"/changerealm {worldName}"
        });
    }

    public void GoToMagic()
    {
        WebInterface.GoToMagic();
    }

    public void Dispose()
    {
    }

    public void Initialize()
    {

    }

}

