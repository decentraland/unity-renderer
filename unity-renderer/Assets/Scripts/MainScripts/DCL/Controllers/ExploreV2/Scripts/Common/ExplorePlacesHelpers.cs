using DCL;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;

public static class ExplorePlacesHelpers
{
    internal const string PLACE_CARD_MODAL_ID = "PlaceCard_Modal";
    internal const string NO_PLACE_DESCRIPTION_WRITTEN = "The author hasn't written a description yet.";

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
        }
    }

    public static List<BaseComponentView> InstantiateAndConfigurePlaceCards(
        List<PlaceCardComponentModel> places,
        Pool pool,
        Action<FriendsHandler> OnFriendHandlerAdded,
        Action<PlaceCardComponentModel> OnPlaceInfoClicked,
        Action<HotScenesController.HotSceneInfo> OnPlaceJumpInClicked)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (PlaceCardComponentModel placeInfo in places)
        {
            PlaceCardComponentView placeGO = pool.Get().gameObject.GetComponent<PlaceCardComponentView>();
            ConfigurePlaceCard(placeGO, placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked);
            OnFriendHandlerAdded?.Invoke(placeGO.friendsHandler);
            instantiatedPlaces.Add(placeGO);
        }

        return instantiatedPlaces;
    }

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
            WebInterface.GoTo(placeFromAPI.baseCoords.x, placeFromAPI.baseCoords.y);
        else
            WebInterface.JumpIn(placeFromAPI.baseCoords.x, placeFromAPI.baseCoords.y, realm.serverName, realm.layer);
    }
}