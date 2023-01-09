using System;
using System.Linq;
using static HotScenesController;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the places management in ExploreV2.
/// </summary>
public static class PlacesCardsConfigurator
{
    private const string NO_PLACE_DESCRIPTION_WRITTEN = "The author hasn't written a description yet.";

    /// <summary>
    /// Makes a jump in to the place defined by the given place data from API.
    /// </summary>
    /// <param name="placeFromAPI">Place data from API.</param>
    public static void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        HotSceneInfo.Realm realm = new HotSceneInfo.Realm() { layer = null, serverName = null };
        placeFromAPI.realms = placeFromAPI.realms.OrderByDescending(x => x.usersCount).ToArray();

        for (int i = 0; i < placeFromAPI.realms.Length; i++)
        {
            bool isArchipelagoRealm = string.IsNullOrEmpty(placeFromAPI.realms[i].layer);

            if (isArchipelagoRealm || placeFromAPI.realms[i].usersCount < placeFromAPI.realms[i].maxUsers)
            {
                realm = placeFromAPI.realms[i];
                break;
            }
        }

        if (string.IsNullOrEmpty(realm.serverName))
            Environment.i.world.teleportController.Teleport(placeFromAPI.baseCoords.x, placeFromAPI.baseCoords.y);
        else
            Environment.i.world.teleportController.JumpIn(placeFromAPI.baseCoords.x, placeFromAPI.baseCoords.y, realm.serverName, realm.layer);
    }

    /// <summary>
    /// Configure a place card with the given model.
    /// </summary>
    /// <param name="placeCard">Place card to configure.</param>
    /// <param name="placeInfo">Model to apply.</param>
    /// <param name="OnPlaceInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnPlaceJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    public static PlaceCardComponentView Configure(PlaceCardComponentView placeCard, PlaceCardComponentModel placeInfo, Action<PlaceCardComponentModel> OnPlaceInfoClicked, Action<HotSceneInfo> OnPlaceJumpInClicked)
    {
        placeCard.Configure(placeInfo);

        placeCard.onInfoClick?.RemoveAllListeners();
        placeCard.onInfoClick?.AddListener(() => OnPlaceInfoClicked?.Invoke(placeInfo));
        placeCard.onJumpInClick?.RemoveAllListeners();
        placeCard.onJumpInClick?.AddListener(() => OnPlaceJumpInClicked?.Invoke(placeInfo.hotSceneInfo));

        return placeCard;
    }

    public static PlaceCardComponentModel ConfigureFromAPIData(PlaceCardComponentModel cardModel, HotSceneInfo placeFromAPI)
    {
        cardModel.placePictureUri = placeFromAPI.thumbnail;
        cardModel.placeName = placeFromAPI.name;
        cardModel.placeDescription = FormatDescription(placeFromAPI);
        cardModel.placeAuthor = FormatAuthorName(placeFromAPI);
        cardModel.numberOfUsers = placeFromAPI.usersTotalCount;
        cardModel.parcels = placeFromAPI.parcels;
        cardModel.coords = placeFromAPI.baseCoords;
        cardModel.hotSceneInfo = placeFromAPI;

        return cardModel;
    }

    internal static string FormatDescription(HotSceneInfo placeFromAPI) =>
        string.IsNullOrEmpty(placeFromAPI.description) ? NO_PLACE_DESCRIPTION_WRITTEN : placeFromAPI.description;

    internal static string FormatAuthorName(HotSceneInfo placeFromAPI) =>
        $"Author <b>{placeFromAPI.creator}</b>";
}
