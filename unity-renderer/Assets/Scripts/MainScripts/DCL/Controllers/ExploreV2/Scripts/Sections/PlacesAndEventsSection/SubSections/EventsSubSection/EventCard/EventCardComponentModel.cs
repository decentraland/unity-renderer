using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class EventCardComponentModel
{
    public Sprite eventPicture;
    public bool isLive;
    public string liveTagText;
    public string eventDateText;
    public string eventName;
    public string eventDescription;
    public string eventStartedIn;
    public string eventOrganizer;
    public string eventPlace;
    public int subscribedUsers;
    public bool isSubscribed;
    public JumpInConfig jumpInConfiguration;
    public Button.ButtonClickedEvent onJumpInClick;
    public Button.ButtonClickedEvent onInfoClick;
    public Button.ButtonClickedEvent onSubscribeClick;
    public Button.ButtonClickedEvent onUnsubscribeClick;
}

[Serializable]
public class JumpInConfig
{
    public Vector2Int coords;
    public string serverName;
    public string layerName;
}