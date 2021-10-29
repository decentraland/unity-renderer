using System;
using UnityEngine;

[Serializable]
public class PlaceCardComponentModel : BaseComponentModel
{
    public Sprite placePictureSprite;
    public Texture2D placePictureTexture;
    public string placePictureUri;
    public string placeName;
    public string placeDescription;
    public string placeAuthor;
    public int numberOfUsers;
    public Vector2Int coords;
    public Vector2Int[] parcels;

    [HideInInspector]
    public HotScenesController.HotSceneInfo hotSceneInfo;
}