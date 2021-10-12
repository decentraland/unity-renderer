using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlaceCardComponentModel
{
    public Sprite placePictureSprite;
    public Texture2D placePictureTexture;
    public string placePictureUri;
    public string placeName;
    public string placeDescription;
    public string placeAuthor;
    public int numberOfUsers;
    public Vector2Int[] parcels;
    public Button.ButtonClickedEvent onJumpInClick;
    public Button.ButtonClickedEvent onInfoClick;

    [HideInInspector]
    public HotScenesController.HotSceneInfo hotSceneInfo;
}