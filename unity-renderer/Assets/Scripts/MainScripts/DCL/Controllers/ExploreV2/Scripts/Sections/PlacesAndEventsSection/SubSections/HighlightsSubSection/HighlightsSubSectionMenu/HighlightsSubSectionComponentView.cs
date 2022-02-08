using DCL;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IHighlightsSubSectionComponentView
{
    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

    /// <summary>
    /// It will be triggered when the place info button is clicked.
    /// </summary>
    event Action<PlaceCardComponentModel> OnPlaceInfoClicked;

    /// <summary>
    /// It will be triggered when the event info button is clicked.
    /// </summary>
    event Action<EventCardComponentModel> OnEventInfoClicked;

    /// <summary>
    /// It will be triggered when the place JumpIn button is clicked.
    /// </summary>
    event Action<HotScenesController.HotSceneInfo> OnPlaceJumpInClicked;

    /// <summary>
    /// It will be triggered when the event JumpIn button is clicked.
    /// </summary>
    event Action<EventFromAPIModel> OnEventJumpInClicked;

    /// <summary>
    /// It will be triggered when the subscribe event button is clicked.
    /// </summary>
    event Action<string> OnEventSubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the unsubscribe event button is clicked.
    /// </summary>
    event Action<string> OnEventUnsubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the view all events button is clicked.
    /// </summary>
    event Action OnViewAllEventsClicked;

    /// <summary>
    /// It will be triggered when a new friend handler is added by a place card.
    /// </summary>
    event Action<FriendsHandler> OnFriendHandlerAdded;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnHighlightsSubSectionEnable;

    /// <summary>
    /// Colors used for the background of the friends heads.
    /// </summary>
    Color[] currentFriendColors { get; }

    /// <summary>
    /// Set the trending places/events component with a list of places and events.
    /// </summary>
    /// <param name="places">List of places (model) to be loaded.</param>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetTrendingPlacesAndEvents(List<PlaceCardComponentModel> places, List<EventCardComponentModel> events);

    /// <summary>
    /// Set the trending places and events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetTrendingPlacesAndEventsAsLoading(bool isVisible);

    /// <summary>
    /// Activates/deactivates the trending places and events component.
    /// </summary>
    /// <param name="isActive">True for activating.</param>
    void SetTrendingPlacesAndEventsActive(bool isActive);

    /// <summary>
    /// Set the featured places component with a list of places.
    /// </summary>
    /// <param name="places">List of places (model) to be loaded.</param>
    void SetFeaturedPlaces(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Set the featured places component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetFeaturedPlacesAsLoading(bool isVisible);

    /// <summary>
    /// Set the live events component with a list of places.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetLiveEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the live events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetLiveAsLoading(bool isVisible);

    /// <summary>
    /// Shows the Place Card modal with the provided information.
    /// </summary>
    /// <param name="placeInfo">Place (model) to be loaded in the card.</param>
    void ShowPlaceModal(PlaceCardComponentModel placeInfo);

    /// <summary>
    /// Hides the Place Card modal.
    /// </summary>
    void HidePlaceModal();

    /// <summary>
    /// Shows the Event Card modal with the provided information.
    /// </summary>
    /// <param name="eventInfo">Event (model) to be loaded in the card.</param>
    void ShowEventModal(EventCardComponentModel eventInfo);

    /// <summary>
    /// Hides the Event Card modal.
    /// </summary>
    void HideEventModal();

    /// <summary>
    /// Set the current scroll view position to 1.
    /// </summary>
    void RestartScrollViewPosition();

    /// <summary>
    /// Configure the needed pools for the places and events instantiation.
    /// </summary>
    void ConfigurePools();
}

public class HighlightsSubSectionComponentView : BaseComponentView, IHighlightsSubSectionComponentView
{
    internal const string TRENDING_PLACE_CARDS_POOL_NAME = "Highlights_TrendingPlaceCardsPool";
    internal const int TRENDING_PLACE_CARDS_POOL_PREWARM = 10;
    internal const string TRENDING_EVENT_CARDS_POOL_NAME = "Highlights_TrendingEventCardsPool";
    internal const int TRENDING_EVENT_CARDS_POOL_PREWARM = 10;
    internal const string FEATURED_PLACE_CARDS_POOL_NAME = "Highlights_FeaturedPlaceCardsPool";
    internal const int FEATURED_PLACE_CARDS_POOL_PREWARM = 9;
    internal const string LIVE_EVENT_CARDS_POOL_NAME = "Highlights_LiveEventCardsPool";
    internal const int LIVE_EVENT_CARDS_POOL_PREWARM = 3;

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardLongPrefab;
    [SerializeField] internal EventCardComponentView eventCardLongPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal CarouselComponentView trendingPlacesAndEvents;
    [SerializeField] internal GameObject trendingPlacesAndEventsLoading;
    [SerializeField] internal GridContainerComponentView featuredPlaces;
    [SerializeField] internal GameObject featuredPlacesLoading;
    [SerializeField] internal TMP_Text featuredPlacesNoDataText;
    [SerializeField] internal GridContainerComponentView liveEvents;
    [SerializeField] internal GameObject liveEventsLoading;
    [SerializeField] internal TMP_Text liveEventsNoDataText;
    [SerializeField] internal ButtonComponentView viewAllEventsButton;
    [SerializeField] internal Color[] friendColors = null;

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnPlaceInfoClicked;
    public event Action<EventCardComponentModel> OnEventInfoClicked;
    public event Action<HotScenesController.HotSceneInfo> OnPlaceJumpInClicked;
    public event Action<EventFromAPIModel> OnEventJumpInClicked;
    public event Action<string> OnEventSubscribeEventClicked;
    public event Action<string> OnEventUnsubscribeEventClicked;
    public event Action OnViewAllEventsClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnHighlightsSubSectionEnable;

    internal PlaceCardComponentView placeModal;
    internal EventCardComponentView eventModal;
    internal Pool trendingPlaceCardsPool;
    internal Pool trendingEventCardsPool;
    internal Pool featuredPlaceCardsPool;
    internal Pool liveEventCardsPool;

    public Color[] currentFriendColors => friendColors;

    public override void OnEnable() { OnHighlightsSubSectionEnable?.Invoke(); }

    public void ConfigurePools()
    {
        ExplorePlacesUtils.ConfigurePlaceCardsPool(out trendingPlaceCardsPool, TRENDING_PLACE_CARDS_POOL_NAME, placeCardLongPrefab, TRENDING_PLACE_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out trendingEventCardsPool, TRENDING_EVENT_CARDS_POOL_NAME, eventCardLongPrefab, TRENDING_EVENT_CARDS_POOL_PREWARM);
        ExplorePlacesUtils.ConfigurePlaceCardsPool(out featuredPlaceCardsPool, FEATURED_PLACE_CARDS_POOL_NAME, placeCardPrefab, FEATURED_PLACE_CARDS_POOL_PREWARM);
        ExploreEventsUtils.ConfigureEventCardsPool(out liveEventCardsPool, LIVE_EVENT_CARDS_POOL_NAME, eventCardPrefab, LIVE_EVENT_CARDS_POOL_PREWARM);
    }

    public override void Start()
    {
        placeModal = ExplorePlacesUtils.ConfigurePlaceCardModal(placeCardModalPrefab);
        eventModal = ExploreEventsUtils.ConfigureEventCardModal(eventCardModalPrefab);

        trendingPlacesAndEvents.RemoveItems();
        featuredPlaces.RemoveItems();
        liveEvents.RemoveItems();

        viewAllEventsButton.onClick.AddListener(() => OnViewAllEventsClicked?.Invoke());

        OnReady?.Invoke();
    }

    public override void RefreshControl()
    {
        trendingPlacesAndEvents.RefreshControl();
        featuredPlaces.RefreshControl();
        liveEvents.RefreshControl();
    }

    public override void Dispose()
    {
        base.Dispose();

        trendingPlacesAndEvents.Dispose();
        featuredPlaces.Dispose();
        liveEvents.Dispose();

        if (placeModal != null)
        {
            placeModal.Dispose();
            Destroy(placeModal.gameObject);
        }

        if (eventModal != null)
        {
            eventModal.Dispose();
            Destroy(eventModal.gameObject);
        }

        viewAllEventsButton.onClick.RemoveAllListeners();
    }

    public void SetTrendingPlacesAndEvents(List<PlaceCardComponentModel> places, List<EventCardComponentModel> events)
    {
        List<BaseComponentView> placesAndEventsToSet = new List<BaseComponentView>();

        trendingPlacesAndEvents.ExtractItems();
        trendingPlaceCardsPool.ReleaseAll();
        trendingEventCardsPool.ReleaseAll();

        List<BaseComponentView> placeComponentsToAdd = ExplorePlacesUtils.InstantiateAndConfigurePlaceCards(
            places,
            trendingPlaceCardsPool,
            OnFriendHandlerAdded,
            OnPlaceInfoClicked,
            OnPlaceJumpInClicked);

        List<BaseComponentView> eventComponentsToAdd = ExploreEventsUtils.InstantiateAndConfigureEventCards(
            events,
            trendingEventCardsPool,
            OnEventInfoClicked,
            OnEventJumpInClicked,
            OnEventSubscribeEventClicked,
            OnEventUnsubscribeEventClicked);


        for (int i = 0; i < HighlightsSubSectionComponentController.DEFAULT_NUMBER_OF_TRENDING_PLACES; i++)
        {
            if (eventComponentsToAdd.Count - 1 >= i)
                placesAndEventsToSet.Add(eventComponentsToAdd[i]);

            if (placeComponentsToAdd.Count - 1 >= i)
                placesAndEventsToSet.Add(placeComponentsToAdd[i]);
        }

        trendingPlacesAndEvents.SetItems(placesAndEventsToSet);
        SetTrendingPlacesAndEventsActive(placesAndEventsToSet.Count > 0);
    }

    public void SetTrendingPlacesAndEventsAsLoading(bool isVisible)
    {
        SetTrendingPlacesAndEventsActive(!isVisible);
        trendingPlacesAndEventsLoading.SetActive(isVisible);
    }

    public void SetTrendingPlacesAndEventsActive(bool isActive) { trendingPlacesAndEvents.gameObject.SetActive(isActive); }

    public void SetFeaturedPlaces(List<PlaceCardComponentModel> places)
    {
        featuredPlaces.ExtractItems();
        featuredPlaceCardsPool.ReleaseAll();

        List<BaseComponentView> placeComponentsToAdd = ExplorePlacesUtils.InstantiateAndConfigurePlaceCards(
            places,
            featuredPlaceCardsPool,
            OnFriendHandlerAdded,
            OnPlaceInfoClicked,
            OnPlaceJumpInClicked);

        featuredPlaces.SetItems(placeComponentsToAdd);
        featuredPlacesNoDataText.gameObject.SetActive(places.Count == 0);
    }

    public void SetFeaturedPlacesAsLoading(bool isVisible)
    {
        featuredPlaces.gameObject.SetActive(!isVisible);
        featuredPlacesLoading.SetActive(isVisible);

        if (isVisible)
            featuredPlacesNoDataText.gameObject.SetActive(false);
    }

    public void SetLiveEvents(List<EventCardComponentModel> events)
    {
        liveEvents.ExtractItems();
        liveEventCardsPool.ReleaseAll();

        List<BaseComponentView> eventComponentsToAdd = ExploreEventsUtils.InstantiateAndConfigureEventCards(
            events,
            liveEventCardsPool,
            OnEventInfoClicked,
            OnEventJumpInClicked,
            OnEventSubscribeEventClicked,
            OnEventUnsubscribeEventClicked);

        liveEvents.SetItems(eventComponentsToAdd);
        liveEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetLiveAsLoading(bool isVisible)
    {
        liveEvents.gameObject.SetActive(!isVisible);
        liveEventsLoading.SetActive(isVisible);
        viewAllEventsButton.gameObject.SetActive(!isVisible);

        if (isVisible)
            liveEventsNoDataText.gameObject.SetActive(false);
    }

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.Show();
        ExplorePlacesUtils.ConfigurePlaceCard(placeModal, placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked);
    }

    public void HidePlaceModal() { placeModal.Hide(); }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        ExploreEventsUtils.ConfigureEventCard(eventModal, eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);
    }

    public void HideEventModal() { eventModal.Hide(); }

    public void RestartScrollViewPosition() { scrollView.verticalNormalizedPosition = 1; }
}