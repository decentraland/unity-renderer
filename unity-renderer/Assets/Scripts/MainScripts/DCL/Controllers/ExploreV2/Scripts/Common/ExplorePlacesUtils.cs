using DCL;
using System;
using System.Linq;
using static HotScenesController;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the places management in ExploreV2.
/// </summary>
public static class ExplorePlacesUtils
{

    // /// <summary>
    // /// Configure a place card with the given model.
    // /// </summary>
    // /// <param name="placeCard">Place card to configure.</param>
    // /// <param name="placeInfo">Model to apply.</param>
    // /// <param name="OnPlaceInfoClicked">Action to inform when the Info button has been clicked.</param>
    // /// <param name="OnPlaceJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    // public static void ConfigurePlaceCard(
    //     PlaceCardComponentView placeCard,
    //     PlaceCardComponentModel placeInfo,
    //     Action<PlaceCardComponentModel> OnPlaceInfoClicked,
    //     Action<HotSceneInfo> OnPlaceJumpInClicked)
    // {
    //     placeCard.Configure(placeInfo);
    //
    //     placeCard.onInfoClick?.RemoveAllListeners();
    //     placeCard.onInfoClick?.AddListener(() => OnPlaceInfoClicked?.Invoke(placeInfo));
    //     placeCard.onJumpInClick?.RemoveAllListeners();
    //     placeCard.onJumpInClick?.AddListener(() => OnPlaceJumpInClicked?.Invoke(placeInfo.hotSceneInfo));
    // }

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
}
