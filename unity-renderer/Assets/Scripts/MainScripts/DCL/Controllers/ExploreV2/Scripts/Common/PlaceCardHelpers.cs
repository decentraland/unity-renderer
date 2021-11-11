using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlaceCardHelpers
{
    internal const string PLACE_CARD_MODAL_ID = "PlaceCard_Modal";

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
}