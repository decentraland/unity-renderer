using System;
using UnityEngine;

[Serializable]
public class EventCardComponentModel : BaseComponentModel
{
    public string eventId;
    public Sprite eventPictureSprite;
    public Texture2D eventPictureTexture;
    public string eventPictureUri;
    public bool isLive;
    public string liveTagText;
    public string eventDateText;
    public string eventName;
    public string eventDescription;
    public string eventStartedIn;
    public string eventStartsInFromTo;
    public string eventOrganizer;
    public string eventPlace;
    public int subscribedUsers;
    public bool isSubscribed;
    public Vector2Int coords;
    public int numberOfUsers;
    public string worldAddress;

    [HideInInspector]
    public EventFromAPIModel eventFromAPIInfo;
}
