using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PlacesAndEventsCardsFactory
{
    private const string NO_PLACE_DESCRIPTION_WRITTEN = "The author hasn't written a description yet.";
    internal const string LIVE_TAG_TEXT = "LIVE";

    internal const string EVENT_CARD_MODAL_ID = "EventCard_Modal";
    internal const string PLACE_CARD_MODAL_ID = "PlaceCard_Modal";

    public static TCardView ConfigureCardModalTemplate<TCardView>(TCardView modalPrefab, string cardModalId)
        where TCardView: BaseComponentView
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
    /// Instantiates (if does not already exists) a place card modal from the given prefab.
    /// </summary>
    /// <param name="placeCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a place card modal.</returns>
    public static PlaceCardComponentView ConfigurePlaceCardModal(PlaceCardComponentView placeCardModalPrefab) =>
        ConfigureCardModalTemplate<PlaceCardComponentView>(placeCardModalPrefab, PLACE_CARD_MODAL_ID);

    /// <summary>
    /// Instantiates (if does not already exists) a event card modal from the given prefab.
    /// </summary>
    /// <param name="eventCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a event card modal.</returns>
    public static EventCardComponentView ConfigureEventCardModal(EventCardComponentView eventCardModalPrefab) =>
        ConfigureCardModalTemplate<EventCardComponentView>(eventCardModalPrefab, EVENT_CARD_MODAL_ID);

    public static List<PlaceCardComponentModel> CreatePlacesCards(List<HotScenesController.HotSceneInfo> filteredPlaces) =>
        CreateModelsListFromAPI(filteredPlaces, CreatePlaceCardModelFromAPIPlace);

    public static List<EventCardComponentModel> CreateEventsCards(List<EventFromAPIModel> filteredEvents) =>
        CreateModelsListFromAPI(filteredEvents, CreateEventCardModelFromAPIEvent);

    private static List<TModel> CreateModelsListFromAPI<TModel,TInfo>(List<TInfo> filteredAPIModels, Func<TInfo, TModel> factoryCreateMethod)
        where TModel: BaseComponentModel
    {
        List<TModel> loadedCards = new List<TModel>();

        foreach (TInfo filteredCardInfo in filteredAPIModels)
            loadedCards.Add(factoryCreateMethod(filteredCardInfo));

        return loadedCards;
    }

    internal static PlaceCardComponentModel CreatePlaceCardModelFromAPIPlace(HotScenesController.HotSceneInfo placeFromAPI) =>
        new()
        {
            placePictureUri = placeFromAPI.thumbnail,
            placeName = placeFromAPI.name,
            placeDescription = FormatDescription(placeFromAPI),
            placeAuthor = FormatAuthorName(placeFromAPI),
            numberOfUsers = placeFromAPI.usersTotalCount,
            parcels = placeFromAPI.parcels,
            coords = placeFromAPI.baseCoords,
            hotSceneInfo = placeFromAPI,
        };

    internal static string FormatDescription(HotScenesController.HotSceneInfo placeFromAPI) =>
        string.IsNullOrEmpty(placeFromAPI.description) ? NO_PLACE_DESCRIPTION_WRITTEN : placeFromAPI.description;

    internal static string FormatAuthorName(HotScenesController.HotSceneInfo placeFromAPI) =>
        $"Author <b>{placeFromAPI.creator}</b>";

    /// <summary>
    /// Returns a event card model from the given API data.
    /// </summary>
    /// <param name="eventFromAPI">Data received from the API.</param>
    /// <returns>An event card model.</returns>
    public static EventCardComponentModel CreateEventCardModelFromAPIEvent(EventFromAPIModel eventFromAPI) =>
        new()
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
