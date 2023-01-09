using DCL;
using System;
using System.Linq;
using UnityEngine;
using static HotScenesController;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the places management in ExploreV2.
/// </summary>
public static class ExplorePlacesUtils
{


    /// <summary>
    /// Creates and configures a pool for place cards.
    /// </summary>
    /// <param name="pool">Pool to configure.</param>
    /// <param name="poolName">Name of the pool.</param>
    /// <param name="placeCardPrefab">Place card prefab to use by the pool.</param>
    /// <param name="maxPrewarmCount">Max number of pre-created cards.</param>
    public static void ConfigurePlaceCardsPool(out Pool pool, string poolName, PlaceCardComponentView placeCardPrefab, int maxPrewarmCount)
    {
        pool = PoolManager.i.GetPool(poolName);
        if (pool == null)
        {
            pool = PoolManager.i.AddPool(
                poolName,
                GameObject.Instantiate(placeCardPrefab).gameObject,
                maxPrewarmCount: maxPrewarmCount,
                isPersistent: true);

            pool.ForcePrewarm();
        }
    }

    public static BaseComponentView InstantiateConfiguredPlaceCard(PlaceCardComponentModel placeInfo, Pool pool,
        Action<FriendsHandler> OnFriendHandlerAdded, Action<PlaceCardComponentModel> OnPlaceInfoClicked, Action<HotSceneInfo> OnPlaceJumpInClicked)
    {
        PlaceCardComponentView placeGO = pool.Get().gameObject.GetComponent<PlaceCardComponentView>();
        ConfigurePlaceCard(placeGO, placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked);
        OnFriendHandlerAdded?.Invoke(placeGO.friendsHandler);
        return placeGO;
    }

    /// <summary>
    /// Configure a place card with the given model.
    /// </summary>
    /// <param name="placeCard">Place card to configure.</param>
    /// <param name="placeInfo">Model to apply.</param>
    /// <param name="OnPlaceInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnPlaceJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    public static void ConfigurePlaceCard(
        PlaceCardComponentView placeCard,
        PlaceCardComponentModel placeInfo,
        Action<PlaceCardComponentModel> OnPlaceInfoClicked,
        Action<HotScenesController.HotSceneInfo> OnPlaceJumpInClicked)
    {
        placeCard.Configure(placeInfo);
        placeCard.onInfoClick?.RemoveAllListeners();
        placeCard.onInfoClick?.AddListener(() => OnPlaceInfoClicked?.Invoke(placeInfo));
        placeCard.onJumpInClick?.RemoveAllListeners();
        placeCard.onJumpInClick?.AddListener(() => OnPlaceJumpInClicked?.Invoke(placeInfo.hotSceneInfo));
    }

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
