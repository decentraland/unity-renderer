using System;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

[Serializable]
public class PlaceCardComponentModel : BaseComponentModel
{
    public bool isWorld;
    public Sprite placePictureSprite;
    public Texture2D placePictureTexture;
    public string placePictureUri;
    public string placeName;
    public string placeDescription;
    public string placeAuthor;
    public int numberOfUsers;
    public Vector2Int coords;
    public Vector2Int[] parcels;
    public bool isFavorite;
    public int userVisits;
    public bool isUpvote;
    public bool isDownvote;
    public int totalVotes;
    public float? userRating;
    public int numberOfFavorites;
    public string deployedAt;
    public bool isPOI;

    [HideInInspector]
    public IHotScenesController.PlaceInfo placeInfo;
}
