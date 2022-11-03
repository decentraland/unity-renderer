using DCL;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the places management in ExploreV2.
/// </summary>
public static class ExplorePlacesUtils
{
    internal const string PLACE_CARD_MODAL_ID = "PlaceCard_Modal";
    internal const string NO_PLACE_DESCRIPTION_WRITTEN = "The author hasn't written a description yet.";

    /// <summary>
    /// Instantiates (if does not already exists) a place card modal from the given prefab.
    /// </summary>
    /// <param name="placeCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a place card modal.</returns>
    public static PlaceCardComponentView ConfigurePlaceCardModal(PlaceCardComponentView placeCardModalPrefab)
    {
        PlaceCardComponentView placeModal = null;

        GameObject existingModal = GameObject.Find(PLACE_CARD_MODAL_ID);
        if (existingModal != null)
            placeModal = existingModal.GetComponent<PlaceCardComponentView>();
        else
        {
            placeModal = GameObject.Instantiate(placeCardModalPrefab);
            placeModal.name = PLACE_CARD_MODAL_ID;
        }

        placeModal.Hide(true);

        return placeModal;
    }

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

    /// <summary>
    /// Instantiates and configures a given list of places.
    /// </summary>
    /// <param name="places">List of places data.</param>
    /// <param name="pool">Pool to use.</param>
    /// <param name="OnFriendHandlerAdded">Action to inform about the addition of a new friend handler.</param>
    /// <param name="OnPlaceInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnPlaceJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    /// <returns>A list of instances of places.</returns>
    public static List<BaseComponentView> InstantiateAndConfigurePlaceCards(
        List<PlaceCardComponentModel> places,
        Pool pool,
        Action<FriendsHandler> OnFriendHandlerAdded,
        Action<PlaceCardComponentModel> OnPlaceInfoClicked,
        Action<HotScenesController.HotSceneInfo> OnPlaceJumpInClicked)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (PlaceCardComponentModel placeInfo in places)
            instantiatedPlaces.Add(
                InstantiateConfiguredPlaceCard(placeInfo, pool, OnFriendHandlerAdded, OnPlaceInfoClicked, OnPlaceJumpInClicked)
                );

        return instantiatedPlaces;
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
    /// Returs a place card model from the given API data.
    /// </summary>
    /// <param name="placeFromAPI">Data received from the API.</param>
    /// <returns>A place card model.</returns>
    public static PlaceCardComponentModel CreatePlaceCardModelFromAPIPlace(HotSceneInfo placeFromAPI)
    {
        PlaceCardComponentModel placeCardModel = new PlaceCardComponentModel();
        placeCardModel.placePictureUri = placeFromAPI.thumbnail;
        placeCardModel.placeName = placeFromAPI.name;
        placeCardModel.placeDescription = FormatDescription(placeFromAPI);
        placeCardModel.placeAuthor = FormatAuthorName(placeFromAPI);
        placeCardModel.numberOfUsers = placeFromAPI.usersTotalCount;
        placeCardModel.parcels = placeFromAPI.parcels;
        placeCardModel.coords = placeFromAPI.baseCoords;
        placeCardModel.hotSceneInfo = placeFromAPI;

        return placeCardModel;
    }

    internal static string FormatDescription(HotSceneInfo placeFromAPI) { return string.IsNullOrEmpty(placeFromAPI.description) ? NO_PLACE_DESCRIPTION_WRITTEN : placeFromAPI.description; }

    internal static string FormatAuthorName(HotSceneInfo placeFromAPI) { return $"Author <b>{placeFromAPI.creator}</b>"; }

    /// <summary>
    /// Makes a jump in to the place defined by the given place data from API.
    /// </summary>
    /// <param name="placeFromAPI">Place data from API.</param>
    public static void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        HotScenesController.HotSceneInfo.Realm realm = new HotScenesController.HotSceneInfo.Realm() { layer = null, serverName = null };
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