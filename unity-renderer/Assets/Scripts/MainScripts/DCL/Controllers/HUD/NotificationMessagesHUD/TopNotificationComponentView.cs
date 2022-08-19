using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class TopNotificationComponentView : BaseComponentView, ITopNotificationsComponentView
{
    public static TopNotificationComponentView Create()
    {
        return null;
        //return Instantiate(Resources.Load<TopNotificationComponentView>("SocialBarV1/TopNotificationHUD"));
    }

    public void AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null)
    { 
    
    }

    public override void Show(bool instant = false) 
    { 
    
    }

    public override void Hide(bool instant = false)
    {

    }

    public override void RefreshControl()
    {

    }
}
