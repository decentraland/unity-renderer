using DCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static HotScenesController;
using Object = UnityEngine.Object;

public static class PlacesAndEventsCardsFactory
{
    public static EventCardComponentView CreateConfiguredEventCard(Pool pool, EventCardComponentModel eventInfo, Action<EventCardComponentModel> OnEventInfoClicked, Action<EventFromAPIModel> OnEventJumpInClicked, Action<string> OnEventSubscribeEventClicked,
        Action<string> OnEventUnsubscribeEventClicked) =>
        ConfigureEventCard(pool.Get<EventCardComponentView>(), eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);

    public static PlaceCardComponentView CreateConfiguredPlaceCard(Pool pool, PlaceCardComponentModel placeInfo, Action<PlaceCardComponentModel> OnPlaceInfoClicked, Action<HotSceneInfo> OnPlaceJumpInClicked) =>
        ConfigurePlaceCard(pool.Get<PlaceCardComponentView>(), placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked);

    /// <summary>
    /// Configure a event card with the given model.
    /// </summary>
    /// <param name="eventCard">Event card to configure.</param>
    /// <param name="eventInfo">Model to apply.</param>
    /// <param name="OnEventInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnEventJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    /// <param name="OnEventSubscribeEventClicked">Action to inform when the Subscribe button has been clicked.</param>
    /// <param name="OnEventUnsubscribeEventClicked">Action to inform when the Unsubscribe button has been clicked.</param>
    public static EventCardComponentView ConfigureEventCard(EventCardComponentView eventCard, EventCardComponentModel eventInfo, Action<EventCardComponentModel> OnEventInfoClicked, Action<EventFromAPIModel> OnEventJumpInClicked, Action<string> OnEventSubscribeEventClicked,
        Action<string> OnEventUnsubscribeEventClicked)
    {
        eventCard.Configure(eventInfo);

        eventCard.onInfoClick?.RemoveAllListeners();
        eventCard.onInfoClick?.AddListener(() => OnEventInfoClicked?.Invoke(eventInfo));
        eventCard.onJumpInClick?.RemoveAllListeners();
        eventCard.onJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onJumpInForNotLiveClick?.RemoveAllListeners();
        eventCard.onJumpInForNotLiveClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onSubscribeClick?.RemoveAllListeners();
        eventCard.onSubscribeClick?.AddListener(() => OnEventSubscribeEventClicked?.Invoke(eventInfo.eventId));
        eventCard.onUnsubscribeClick?.RemoveAllListeners();
        eventCard.onUnsubscribeClick?.AddListener(() => OnEventUnsubscribeEventClicked?.Invoke(eventInfo.eventId));

        return eventCard;
    }

    /// <summary>
    /// Configure a place card with the given model.
    /// </summary>
    /// <param name="placeCard">Place card to configure.</param>
    /// <param name="placeInfo">Model to apply.</param>
    /// <param name="OnPlaceInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnPlaceJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    public static PlaceCardComponentView ConfigurePlaceCard(PlaceCardComponentView placeCard, PlaceCardComponentModel placeInfo, Action<PlaceCardComponentModel> OnPlaceInfoClicked, Action<HotSceneInfo> OnPlaceJumpInClicked)
    {
        placeCard.Configure(placeInfo);

        placeCard.onInfoClick?.RemoveAllListeners();
        placeCard.onInfoClick?.AddListener(() => OnPlaceInfoClicked?.Invoke(placeInfo));
        placeCard.onJumpInClick?.RemoveAllListeners();
        placeCard.onJumpInClick?.AddListener(() => OnPlaceJumpInClicked?.Invoke(placeInfo.hotSceneInfo));

        return placeCard;
    }

    internal const string EVENT_CARD_MODAL_ID = "EventCard_Modal";
    internal const string PLACE_CARD_MODAL_ID = "PlaceCard_Modal";

    /// <summary>
    /// Creates and configures a pool for cards.
    /// </summary>
    /// <param name="pool">Pool to configure.</param>
    /// <param name="poolName">Name of the pool.</param>
    /// <param name="cardPrefab">Card prefab to use by the pool.</param>
    /// <param name="maxPrewarmCount">Max number of pre-created cards.</param>
    public static void ConfigureCardsPool(out Pool pool, string poolName, BaseComponentView cardPrefab, int maxPrewarmCount)
    {
        pool = PoolManager.i.GetPool(poolName);

        if (pool != null) return;

        pool = PoolManager.i.AddPool(poolName, Object.Instantiate(cardPrefab).gameObject, maxPrewarmCount: maxPrewarmCount, isPersistent: true);
        pool.ForcePrewarm();
    }

    /// <summary>
    /// Instantiates (if does not already exists) a place card modal from the given prefab.
    /// </summary>
    /// <param name="placeCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a place card modal.</returns>
    public static PlaceCardComponentView GetOrCreatePlaceCardTemplateHidden(PlaceCardComponentView placeCardModalPrefab) =>
        GetOrCreateCardTemplateHidden(placeCardModalPrefab, PLACE_CARD_MODAL_ID);

    /// <summary>
    /// Instantiates (if does not already exists) a event card modal from the given prefab.
    /// </summary>
    /// <param name="eventCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a event card modal.</returns>
    public static EventCardComponentView GetOrCreateEventCardTemplateHidden(EventCardComponentView eventCardModalPrefab) =>
        GetOrCreateCardTemplateHidden(eventCardModalPrefab, EVENT_CARD_MODAL_ID);

    private static TCardView GetOrCreateCardTemplateHidden<TCardView>(TCardView modalPrefab, string cardModalId) where TCardView: BaseComponentView
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
        CreateModelsListFromAPI<PlaceCardComponentModel, HotSceneInfo>(filteredPlaces, ConfigurePlaceCardModelFromAPIData);

    /// <summary>
    /// Returns a event card model from the given API data.
    /// </summary>
    /// <param name="filteredEvents">Data received from the API.</param>
    /// <returns>An event card model.</returns>
    public static List<EventCardComponentModel> CreateEventsCards(List<EventFromAPIModel> filteredEvents) =>
        CreateModelsListFromAPI<EventCardComponentModel, EventFromAPIModel>(filteredEvents, ConfigureEventCardModelFromAPIData);

    private static List<TModel> CreateModelsListFromAPI<TModel, TInfo>(List<TInfo> filteredAPIModels, Func<TModel, TInfo, TModel> factoryCreateMethod)
        where TModel: BaseComponentModel, new()
    {
        List<TModel> loadedCards = new List<TModel>();

        foreach (TInfo filteredCardInfo in filteredAPIModels)
            loadedCards.Add(factoryCreateMethod(new TModel(), filteredCardInfo));

        return loadedCards;
    }

    // ============= CONFIGURATORS
    private const string NO_PLACE_DESCRIPTION_WRITTEN = "The author hasn't written a description yet.";
    internal const string LIVE_TAG_TEXT = "LIVE";

    internal static PlaceCardComponentModel ConfigurePlaceCardModelFromAPIData(PlaceCardComponentModel cardModel, HotSceneInfo placeFromAPI)
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

    /// <summary>
    /// Returns a event card model from the given API data.
    /// </summary>
    /// <param name="eventFromAPI">Data received from the API.</param>
    /// <returns>An event card model.</returns>
    public static EventCardComponentModel CreateEventCardModelFromAPIEvent(EventFromAPIModel eventFromAPI) =>
        new ()
        {
            eventId = eventFromAPI.id,
            eventPictureUri = eventFromAPI.image,
            isLive = eventFromAPI.live,
            liveTagText = LIVE_TAG_TEXT,
            eventDateText = FormatEventDate(eventFromAPI),
            eventName = eventFromAPI.name,
            eventDescription = eventFromAPI.description,
            eventStartedIn = FormatEventStartDate(eventFromAPI),
            eventStartsInFromTo = FormatEventStartDateFromTo(eventFromAPI),
            eventOrganizer = FormatEventOrganized(eventFromAPI),
            eventPlace = FormatEventPlace(eventFromAPI),
            subscribedUsers = eventFromAPI.total_attendees,
            isSubscribed = false,
            coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]),
            eventFromAPIInfo = eventFromAPI,
        };

    public static EventCardComponentModel ConfigureEventCardModelFromAPIData(EventCardComponentModel cardModel, EventFromAPIModel eventFromAPI)
    {
        cardModel.eventId = eventFromAPI.id;
        cardModel.eventPictureUri = eventFromAPI.image;
        cardModel.isLive = eventFromAPI.live;
        cardModel.liveTagText = LIVE_TAG_TEXT;
        cardModel.eventDateText = FormatEventDate(eventFromAPI);
        cardModel.eventName = eventFromAPI.name;
        cardModel.eventDescription = eventFromAPI.description;
        cardModel.eventStartedIn = FormatEventStartDate(eventFromAPI);
        cardModel.eventStartsInFromTo = FormatEventStartDateFromTo(eventFromAPI);
        cardModel.eventOrganizer = FormatEventOrganized(eventFromAPI);
        cardModel.eventPlace = FormatEventPlace(eventFromAPI);
        cardModel.subscribedUsers = eventFromAPI.total_attendees;
        cardModel.isSubscribed = false;
        cardModel.coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        cardModel.eventFromAPIInfo = eventFromAPI;

        return cardModel;
    }

    internal static string FormatEventDate(EventFromAPIModel eventFromAPI)
    {
        DateTime eventDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        return eventDateTime.ToString("MMMM d", new CultureInfo("en-US"));
    }

    internal static string FormatEventStartDate(EventFromAPIModel eventFromAPI)
    {
        DateTime eventDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        string formattedDate;

        if (eventFromAPI.live)
        {
            int daysAgo = (int)Math.Ceiling((DateTime.Now - eventDateTime).TotalDays);
            int hoursAgo = (int)Math.Ceiling((DateTime.Now - eventDateTime).TotalHours);

            if (daysAgo > 0)
                formattedDate = $"{daysAgo} days ago";
            else
                formattedDate = $"{hoursAgo} hr ago";
        }
        else
        {
            int daysToStart = (int)Math.Ceiling((eventDateTime - DateTime.Now).TotalDays);
            int hoursToStart = (int)Math.Ceiling((eventDateTime - DateTime.Now).TotalHours);

            if (daysToStart > 0)
                formattedDate = $"in {daysToStart} days";
            else
                formattedDate = $"in {hoursToStart} hours";
        }

        return formattedDate;
    }

    internal static string FormatEventStartDateFromTo(EventFromAPIModel eventFromAPI)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        DateTime eventStartDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        DateTime eventEndDateTime = Convert.ToDateTime(eventFromAPI.finish_at).ToUniversalTime();

        string formattedDate = $"From {eventStartDateTime.ToString("dddd", cultureInfo)}, {eventStartDateTime.Day} {eventStartDateTime.ToString("MMM", cultureInfo)}" +
                               $" to {eventEndDateTime.ToString("dddd", cultureInfo)}, {eventEndDateTime.Day} {eventEndDateTime.ToString("MMM", cultureInfo)} UTC";

        return formattedDate;
    }

    internal static string FormatEventOrganized(EventFromAPIModel eventFromAPI) =>
        $"Public, Organized by {eventFromAPI.user_name}";

    internal static string FormatEventPlace(EventFromAPIModel eventFromAPI) =>
        string.IsNullOrEmpty(eventFromAPI.scene_name) ? "Decentraland" : eventFromAPI.scene_name;
}
