using System;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

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
    public IHotScenesController.HotSceneInfo hotSceneInfo;
}
