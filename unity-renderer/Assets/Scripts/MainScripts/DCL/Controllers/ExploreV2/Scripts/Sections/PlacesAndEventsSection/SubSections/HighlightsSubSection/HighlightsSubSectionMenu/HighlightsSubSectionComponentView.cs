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
    /// Set the promoted places component with a list of places.
    /// </summary>
    /// <param name="places">List of places (model) to be loaded.</param>
    void SetPromotedPlaces(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Set the promoted places component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetPromotedPlacessAsLoading(bool isVisible);

    /// <summary>
    /// Activates/deactivates the promoted places component.
    /// </summary>
    /// <param name="isActive">True for activating.</param>
    void SetPromotedPlacesActive(bool isActive);

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
}

public class HighlightsSubSectionComponentView : BaseComponentView, IHighlightsSubSectionComponentView
{
    internal const string PROMOTED_PLACE_CARDS_POOL_NAME = "PromotedPlaceCardsPool";
    internal const string FEATURED_PLACE_CARDS_POOL_NAME = "FeaturedPlaceCardsPool";
    internal const string LIVE_EVENT_CARDS_POOL_NAME = "LiveEventCardsPool";

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardLongPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;
    [SerializeField] internal EventCardComponentView eventCardPrefab;
    [SerializeField] internal EventCardComponentView eventCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal CarouselComponentView promotedPlaces;
    [SerializeField] internal GameObject promotedPlacesLoading;
    [SerializeField] internal GridContainerComponentView featuredPlaces;
    [SerializeField] internal GameObject featuredPlacesLoading;
    [SerializeField] internal TMP_Text featuredPlacesNoDataText;
    [SerializeField] internal GridContainerComponentView liveEvents;
    [SerializeField] internal GameObject liveEventsLoading;
    [SerializeField] internal TMP_Text liveEventsNoDataText;
    [SerializeField] internal Color[] friendColors = null;

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnPlaceInfoClicked;
    public event Action<EventCardComponentModel> OnEventInfoClicked;
    public event Action<HotScenesController.HotSceneInfo> OnPlaceJumpInClicked;
    public event Action<EventFromAPIModel> OnEventJumpInClicked;
    public event Action<string> OnEventSubscribeEventClicked;
    public event Action<string> OnEventUnsubscribeEventClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnHighlightsSubSectionEnable;

    internal PlaceCardComponentView placeModal;
    internal EventCardComponentView eventModal;
    internal Pool promotedPlaceCardsPool;
    internal Pool featuredPlaceCardsPool;
    internal Pool liveEventCardsPool;

    public Color[] currentFriendColors => friendColors;

    public override void OnEnable() { OnHighlightsSubSectionEnable?.Invoke(); }

    public override void Start()
    {
        ConfigurePlaceCardModal();
        ConfigureEventCardModal();
        ConfigurePlaceCardsPool(out promotedPlaceCardsPool, PROMOTED_PLACE_CARDS_POOL_NAME, placeCardLongPrefab, 10);
        ConfigurePlaceCardsPool(out featuredPlaceCardsPool, FEATURED_PLACE_CARDS_POOL_NAME, placeCardPrefab, 6);
        ConfigureEventCardsPool();

        promotedPlaces.RemoveItems();
        featuredPlaces.RemoveItems();
        liveEvents.RemoveItems();

        OnReady?.Invoke();
    }

    public override void RefreshControl()
    {
        promotedPlaces.RefreshControl();
        featuredPlaces.RefreshControl();
        liveEvents.RefreshControl();
    }

    public override void Dispose()
    {
        base.Dispose();

        promotedPlaces.Dispose();
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
    }

    public void SetPromotedPlaces(List<PlaceCardComponentModel> places)
    {
        promotedPlaces.ExtractItems();
        promotedPlaceCardsPool.ReleaseAll();
        List<BaseComponentView> placeComponentsToAdd = InstantiateAndConfigurePlaceCards(places, promotedPlaceCardsPool);
        promotedPlaces.SetItems(placeComponentsToAdd);
        SetPromotedPlacesActive(places.Count > 0);
    }

    public void SetPromotedPlacessAsLoading(bool isVisible)
    {
        SetPromotedPlacesActive(!isVisible);
        promotedPlacesLoading.SetActive(isVisible);
    }

    public void SetPromotedPlacesActive(bool isActive) { promotedPlaces.gameObject.SetActive(isActive); }

    public void SetFeaturedPlaces(List<PlaceCardComponentModel> places)
    {
        featuredPlaces.ExtractItems();
        featuredPlaceCardsPool.ReleaseAll();
        List<BaseComponentView> placeComponentsToAdd = InstantiateAndConfigurePlaceCards(places, featuredPlaceCardsPool);
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
        List<BaseComponentView> eventComponentsToAdd = InstantiateAndConfigureEventCards(events);
        liveEvents.SetItems(eventComponentsToAdd);
        liveEventsNoDataText.gameObject.SetActive(events.Count == 0);
    }

    public void SetLiveAsLoading(bool isVisible)
    {
        liveEvents.gameObject.SetActive(!isVisible);
        liveEventsLoading.SetActive(isVisible);

        if (isVisible)
            liveEventsNoDataText.gameObject.SetActive(false);
    }

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.Show();
        ConfigurePlaceCard(placeModal, placeInfo);
    }

    public void HidePlaceModal() { placeModal.Hide(); }

    public void ShowEventModal(EventCardComponentModel eventInfo)
    {
        eventModal.Show();
        ConfigureEventCard(eventModal, eventInfo);
    }

    public void HideEventModal() { eventModal.Hide(); }

    public void RestartScrollViewPosition() { scrollView.verticalNormalizedPosition = 1; }

    internal void ConfigurePlaceCardModal()
    {
        placeModal = Instantiate(placeCardModalPrefab);
        placeModal.Hide(true);
    }

    internal void ConfigureEventCardModal()
    {
        eventModal = GameObject.Instantiate(eventCardModalPrefab);
        eventModal.Hide(true);
    }

    internal void ConfigurePlaceCardsPool(out Pool pool, string poolName, PlaceCardComponentView placeCardPrefab, int maxPrewarmCount)
    {
        pool = PoolManager.i.GetPool(poolName);
        if (pool == null)
        {
            pool = PoolManager.i.AddPool(
                poolName,
                Instantiate(placeCardPrefab).gameObject,
                maxPrewarmCount: maxPrewarmCount,
                isPersistent: true);
        }
    }

    internal void ConfigureEventCardsPool()
    {
        liveEventCardsPool = PoolManager.i.GetPool(LIVE_EVENT_CARDS_POOL_NAME);
        if (liveEventCardsPool == null)
        {
            liveEventCardsPool = PoolManager.i.AddPool(
                LIVE_EVENT_CARDS_POOL_NAME,
                Instantiate(eventCardPrefab).gameObject,
                maxPrewarmCount: 200,
                isPersistent: true);
        }
    }

    internal List<BaseComponentView> InstantiateAndConfigurePlaceCards(List<PlaceCardComponentModel> places, Pool pool)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (PlaceCardComponentModel placeInfo in places)
        {
            PlaceCardComponentView placeGO = pool.Get().gameObject.GetComponent<PlaceCardComponentView>();
            ConfigurePlaceCard(placeGO, placeInfo);
            OnFriendHandlerAdded?.Invoke(placeGO.friendsHandler);
            instantiatedPlaces.Add(placeGO);
        }

        return instantiatedPlaces;
    }

    internal List<BaseComponentView> InstantiateAndConfigureEventCards(List<EventCardComponentModel> events)
    {
        List<BaseComponentView> instantiatedEvents = new List<BaseComponentView>();

        foreach (EventCardComponentModel eventInfo in events)
        {
            EventCardComponentView eventGO = liveEventCardsPool.Get().gameObject.GetComponent<EventCardComponentView>();
            ConfigureEventCard(eventGO, eventInfo);
            instantiatedEvents.Add(eventGO);
        }

        return instantiatedEvents;
    }

    internal void ConfigurePlaceCard(PlaceCardComponentView placeCard, PlaceCardComponentModel placeInfo)
    {
        placeCard.Configure(placeInfo);
        placeCard.onInfoClick?.RemoveAllListeners();
        placeCard.onInfoClick?.AddListener(() => OnPlaceInfoClicked?.Invoke(placeInfo));
        placeCard.onJumpInClick?.RemoveAllListeners();
        placeCard.onJumpInClick?.AddListener(() => OnPlaceJumpInClicked?.Invoke(placeInfo.hotSceneInfo));
    }

    internal void ConfigureEventCard(EventCardComponentView eventCard, EventCardComponentModel eventInfo)
    {
        eventCard.Configure(eventInfo);
        eventCard.onInfoClick?.RemoveAllListeners();
        eventCard.onInfoClick?.AddListener(() => OnEventInfoClicked?.Invoke(eventInfo));
        eventCard.onJumpInClick?.RemoveAllListeners();
        eventCard.onJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onSubscribeClick?.RemoveAllListeners();
        eventCard.onSubscribeClick?.AddListener(() => OnEventSubscribeEventClicked?.Invoke(eventInfo.eventId));
        eventCard.onUnsubscribeClick?.RemoveAllListeners();
        eventCard.onUnsubscribeClick?.AddListener(() => OnEventUnsubscribeEventClicked?.Invoke(eventInfo.eventId));
    }
}