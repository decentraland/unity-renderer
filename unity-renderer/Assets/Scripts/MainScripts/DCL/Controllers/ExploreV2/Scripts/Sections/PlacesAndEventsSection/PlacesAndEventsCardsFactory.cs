using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HotScenesController;
using Object = UnityEngine.Object;

public static class PlacesAndEventsCardsFactory
{
    internal const string EVENT_CARD_MODAL_ID = "EventCard_Modal";
    internal const string PLACE_CARD_MODAL_ID = "PlaceCard_Modal";

    /// <summary>
    /// Creates and configures a pool for cards.
    /// </summary>
    /// <param name="pool">Pool to configure.</param>
    /// <param name="poolName">Name of the pool.</param>
    /// <param name="cardPrefab">Card prefab to use by the pool.</param>
    /// <param name="maxPrewarmCount">Max number of pre-created cards.</param>
    public static Pool GetCardsPoolLazy(string poolName, BaseComponentView cardPrefab, int maxPrewarmCount)
    {
        Pool pool = PoolManager.i.GetPool(poolName);

        if (pool != null)
            return pool;

        pool = PoolManager.i.AddPool(poolName, Object.Instantiate(cardPrefab).gameObject, maxPrewarmCount: maxPrewarmCount, isPersistent: true);
        pool.ForcePrewarm();

        return pool;
    }

    public static EventCardComponentView CreateConfiguredEventCard(Pool pool, EventCardComponentModel eventInfo, Action<EventCardComponentModel> OnEventInfoClicked, Action<EventFromAPIModel> OnEventJumpInClicked, Action<string> OnEventSubscribeEventClicked, Action<string> OnEventUnsubscribeEventClicked) =>
        EventsCardsConfigurator.Configure(pool.Get<EventCardComponentView>(), eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);

    public static PlaceCardComponentView CreateConfiguredPlaceCard(Pool pool, PlaceCardComponentModel placeInfo, Action<PlaceCardComponentModel> OnPlaceInfoClicked, Action<HotSceneInfo> OnPlaceJumpInClicked) =>
        PlacesCardsConfigurator.Configure(pool.Get<PlaceCardComponentView>(), placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked);

    /// <summary>
    /// Instantiates (if does not already exists) a place card modal from the given prefab.
    /// </summary>
    /// <param name="placeCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a place card modal.</returns>
    public static PlaceCardComponentView GetPlaceCardTemplateHiddenLazy(PlaceCardComponentView placeCardModalPrefab) =>
        GetCardTemplateHiddenLazy(placeCardModalPrefab, PLACE_CARD_MODAL_ID);

    /// <summary>
    /// Instantiates (if does not already exists) a event card modal from the given prefab.
    /// </summary>
    /// <param name="eventCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a event card modal.</returns>
    public static EventCardComponentView GetEventCardTemplateHiddenLazy(EventCardComponentView eventCardModalPrefab) =>
        GetCardTemplateHiddenLazy(eventCardModalPrefab, EVENT_CARD_MODAL_ID);

    private static TCardView GetCardTemplateHiddenLazy<TCardView>(TCardView modalPrefab, string cardModalId) where TCardView: BaseComponentView
    {
        TCardView modal;

        GameObject existingModal = GameObject.Find(cardModalId);

        if (existingModal != null)
            modal = existingModal.GetComponent<TCardView>();
        else
        {
            modal = Object.Instantiate(modalPrefab);
            modal.name = cardModalId;
        }

        modal.Hide(true);

        return modal;
    }

    /// <summary>
    /// Returns a place card model from the given API data.
    /// </summary>
    /// <param name="filteredPlaces">Data received from the API.</param>
    /// <returns>A place card model.</returns>
    public static List<PlaceCardComponentModel> CreatePlacesCards(List<HotSceneInfo> filteredPlaces) =>
        CreateModelsListFromAPI<PlaceCardComponentModel, HotSceneInfo>(filteredPlaces, PlacesCardsConfigurator.ConfigureFromAPIData);

    /// <summary>
    /// Returns a event card model from the given API data.
    /// </summary>
    /// <param name="filteredEvents">Data received from the API.</param>
    /// <returns>An event card model.</returns>
    public static List<EventCardComponentModel> CreateEventsCards(List<EventFromAPIModel> filteredEvents) =>
        CreateModelsListFromAPI<EventCardComponentModel, EventFromAPIModel>(filteredEvents, EventsCardsConfigurator.ConfigureFromAPIData);

    private static List<TModel> CreateModelsListFromAPI<TModel, TInfo>(List<TInfo> filteredAPIModels, Func<TModel, TInfo, TModel> configureModelByApiData)
        where TModel: BaseComponentModel, new()
    {
        List<TModel> loadedCards = new List<TModel>();

        foreach (TInfo filteredCardInfo in filteredAPIModels)
            loadedCards.Add(configureModelByApiData(new TModel(), filteredCardInfo));

        return loadedCards;
    }
}
