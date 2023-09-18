using DCL;
using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IEventCardComponentView
{
    /// <summary>
    /// Event that will be triggered when the jumpIn button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onJumpInClick { get; }
    Button.ButtonClickedEvent onSecondaryJumpInClick { get; }

    /// <summary>
    /// Event that will be triggered when the info button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onInfoClick { get; }

    /// <summary>
    /// Event that will be triggered when the background button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onBackgroundClick { get; }

    /// <summary>
    /// Event that will be triggered when the subscribe event button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onSubscribeClick { get; }

    /// <summary>
    /// Event that will be triggered when the unsubscribe event button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onUnsubscribeClick { get; }

    /// <summary>
    /// Set the event picture directly from a sprite.
    /// </summary>
    /// <param name="sprite">Event picture (sprite).</param>
    void SetEventPicture(Sprite sprite);

    /// <summary>
    /// Set the event picture from a 2D texture.
    /// </summary>
    /// <param name="texture">Event picture (url).</param>
    void SetEventPicture(Texture2D texture);

    /// <summary>
    /// Set the event picture from an uri.
    /// </summary>
    /// <param name="uri"></param>
    void SetEventPicture(string uri);

    /// <summary>
    /// Set the event card as live mode.
    /// </summary>
    /// <param name="isLive">True to set the event as live.</param>
    void SetEventAsLive(bool isLive);

    /// <summary>
    /// Set the the number of users in the event.
    /// </summary>
    /// <param name="newNumberOfUsers">Number of users.</param>
    void SetNumberOfUsers(int newNumberOfUsers);

    /// <summary>
    /// Set the live tag text.
    /// </summary>
    /// <param name="newText">New text.</param>
    void SetLiveTagText(string newText);

    /// <summary>
    /// Set the event date.
    /// </summary>
    /// <param name="newDate">The new date showed in the event card.</param>
    void SetEventDate(string newDate);

    /// <summary>
    /// Set the event name in the card.
    /// </summary>
    /// <param name="newText">New event name.</param>
    void SetEventName(string newText);

    /// <summary>
    /// Set the event description in the card.
    /// </summary>
    /// <param name="newText">New event description.</param>
    void SetEventDescription(string newText);

    /// <summary>
    /// Set the event started time in the card.
    /// </summary>
    /// <param name="newText">New event started time.</param>
    void SetEventStartedIn(string newText);

    /// <summary>
    /// Set the event dates range in the card.
    /// </summary>
    /// <param name="newText">New event date range.</param>
    void SetEventStartsInFromTo(string newText);

    /// <summary>
    /// Set the event organizer in the card.
    /// </summary>
    /// <param name="newText">New event organizer.</param>
    void SetEventOrganizer(string newText);

    /// <summary>
    /// Set the event place in the card.
    /// </summary>
    /// <param name="newText">New event place.</param>
    /// <param name="isWorld">True for setting the place as world.</param>
    void SetEventPlace(string newText, bool isWorld);

    /// <summary>
    /// Set the the number of users subscribed to the event.
    /// </summary>
    /// <param name="newNumberOfUsers">Number of users subscribed.</param>
    void SetSubscribersUsers(int newNumberOfUsers);

    /// <summary>
    /// Set the event coords.
    /// </summary>
    /// <param name="newCoords">Event coords.</param>
    /// <param name="worldAddress">World address (for world's events)</param>
    void SetCoords(Vector2Int newCoords, string worldAddress);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator and hiding the card info.</param>
    void SetLoadingIndicatorVisible(bool isVisible);
}

public class EventCardComponentView : BaseComponentView, IEventCardComponentView, IComponentModelConfig<EventCardComponentModel>
{
    internal const string USERS_CONFIRMED_MESSAGE = "{0} interested";
    internal const string NOBODY_CONFIRMED_MESSAGE = "Nobody confirmed yet";
    private const string NO_DESCRIPTION_TEXT = "No description.";
    private const int EVENT_TITLE_LENGTH_LIMIT = 65;

    [Header("Assets References")]
    [SerializeField] internal UserProfile ownUserProfile;

    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView eventImage;
    [SerializeField] internal GameObject numberOfUsersContainer;
    [SerializeField] internal TMP_Text numberOfUsersText;
    [SerializeField] internal TagComponentView liveTag;
    [SerializeField] internal TMP_Text eventDateText;
    [SerializeField] internal TMP_Text eventDateTextOnFocus;
    [SerializeField] internal TMP_Text eventNameText;
    [SerializeField] internal TMP_Text eventNameTextOnFocus;
    [SerializeField] internal TMP_Text eventDescText;
    [SerializeField] internal TMP_Text eventStartedInTitleForLive;
    [SerializeField] internal TMP_Text eventStartedInTitleForLiveOnFocus;
    [SerializeField] internal TMP_Text eventStartedInTitleForNotLive;
    [SerializeField] internal TMP_Text eventStartedInText;
    [SerializeField] internal TMP_Text eventStartsInFromToText;
    [SerializeField] internal TMP_Text eventOrganizerText;
    [SerializeField] internal TMP_Text eventPlaceText;
    [SerializeField] internal TMP_Text subscribedUsersTitleForLive;
    [SerializeField] internal TMP_Text subscribedUsersTitleForNotLive;
    [SerializeField] internal GameObject goingUsersGameObject;
    [SerializeField] internal TMP_Text subscribedUsersText;
    [SerializeField] internal Button modalBackgroundButton;
    [SerializeField] internal ButtonComponentView closeCardButton;
    [SerializeField] internal InputAction_Trigger closeAction;
    [SerializeField] internal ButtonComponentView backgroundButton;
    [SerializeField] internal ButtonComponentView infoButton;
    [SerializeField] internal ButtonComponentView jumpinButton;
    [SerializeField] internal ButtonComponentView secondaryJumpinButton;
    [SerializeField] internal ButtonComponentView subscribeEventButton;
    [SerializeField] internal ButtonComponentView unsubscribeEventButton;
    [SerializeField] internal GameObject imageContainer;
    [SerializeField] internal GameObject eventInfoContainer;
    [SerializeField] internal GameObject loadingSpinner;
    [SerializeField] internal GameObject cardSelectionFrame;
    [SerializeField] internal VerticalLayoutGroup contentVerticalLayout;
    [SerializeField] internal VerticalLayoutGroup infoVerticalLayout;
    [SerializeField] internal HorizontalLayoutGroup timeAndPlayersHorizontalLayout;
    [SerializeField] internal EventCardAnimator cardAnimator;
    [SerializeField] internal ScrollRect scroll;
    [SerializeField] internal GameObject placeIcon;
    [SerializeField] internal GameObject worldIcon;

    [Header("Configuration")]
    [SerializeField] internal Sprite defaultPicture;
    [SerializeField] internal bool isEventCardModal;
    [SerializeField] internal EventCardComponentModel model;

    public Button.ButtonClickedEvent onJumpInClick => jumpinButton?.onClick;
    public Button.ButtonClickedEvent onSecondaryJumpInClick => secondaryJumpinButton?.onClick;
    public Button.ButtonClickedEvent onInfoClick => infoButton?.onClick;
    public Button.ButtonClickedEvent onBackgroundClick => backgroundButton?.onClick;
    public Button.ButtonClickedEvent onSubscribeClick => subscribeEventButton?.onClick;
    public Button.ButtonClickedEvent onUnsubscribeClick => unsubscribeEventButton?.onClick;

    public void Start()
    {
        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);

        if (closeCardButton != null)
            closeCardButton.onClick.AddListener(CloseModal);

        if (closeAction != null)
            closeAction.OnTriggered += OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.AddListener(CloseModal);

        if (secondaryJumpinButton != null)
            secondaryJumpinButton.onClick.AddListener(CloseModal);

        onSubscribeClick.AddListener(PressedSubscribe);
        onUnsubscribeClick.AddListener(PressedUnsubscribe);
    }

    private void PressedSubscribe()
    {
        if (ownUserProfile.isGuest)
            return;

        model.isSubscribed = true;
        model.eventFromAPIInfo.attending = true;
        RefreshControl();
    }

    private void PressedUnsubscribe()
    {
        if (ownUserProfile.isGuest)
            return;

        model.isSubscribed = false;
        model.eventFromAPIInfo.attending = false;
        RefreshControl();
    }

    public void Configure(EventCardComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        if (model.eventPictureSprite != null)
            SetEventPicture(model.eventPictureSprite);
        else if (model.eventPictureTexture != null)
            SetEventPicture(model.eventPictureTexture);
        else if (!string.IsNullOrEmpty(model.eventPictureUri))
            SetEventPicture(model.eventPictureUri);
        else
            SetEventPicture(sprite: null);

        SetEventAsLive(model.isLive);
        SetNumberOfUsers(model.numberOfUsers);
        SetLiveTagText(model.liveTagText);
        SetEventDate(model.eventDateText);
        SetEventName(model.eventName);
        SetEventDescription(model.eventDescription);
        SetEventStartedIn(model.eventStartedIn);
        SetEventStartsInFromTo(model.eventStartsInFromTo);
        SetEventOrganizer(model.eventOrganizer);
        SetEventPlace(model.eventPlace, !string.IsNullOrEmpty(model.worldAddress));
        SetSubscribersUsers(model.subscribedUsers);
        SetCoords(model.coords, model.worldAddress);
        ResetScrollPosition();
        RebuildCardLayouts();
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(true);

        cardAnimator?.Focus();
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);

        cardAnimator?.Idle();
    }

    public override void Show(bool instant = false)
    {
        base.Show(instant);

        DataStore.i.exploreV2.isSomeModalOpen.Set(true);
    }

    public override void Hide(bool instant = false)
    {
        base.Hide(instant);

        DataStore.i.exploreV2.isSomeModalOpen.Set(false);
    }

    public override void Dispose()
    {
        base.Dispose();

        if (eventImage != null)
            eventImage.Dispose();

        if (closeCardButton != null)
            closeCardButton.onClick.RemoveAllListeners();

        if (closeAction != null)
            closeAction.OnTriggered -= OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.RemoveAllListeners();

        if (secondaryJumpinButton != null)
            secondaryJumpinButton.onClick.RemoveAllListeners();

        onSubscribeClick.RemoveAllListeners();
        onUnsubscribeClick.RemoveAllListeners();
    }

    public void SetEventPicture(Sprite sprite)
    {
        if (sprite == null && defaultPicture != null)
            sprite = defaultPicture;

        model.eventPictureSprite = sprite;

        if (eventImage == null)
            return;

        eventImage.SetImage(sprite);
    }

    public void SetEventPicture(Texture2D texture)
    {
        if (texture == null && defaultPicture != null)
        {
            SetEventPicture(defaultPicture);
            return;
        }

        model.eventPictureTexture = texture;

        if (!Application.isPlaying)
            return;

        if (eventImage == null)
            return;

        eventImage.SetImage(texture);
    }

    public void SetEventPicture(string uri)
    {
        if (string.IsNullOrEmpty(uri) && defaultPicture != null)
        {
            SetEventPicture(defaultPicture);
            return;
        }

        model.eventPictureUri = uri;

        if (!Application.isPlaying)
            return;

        if (eventImage == null)
            return;

        eventImage.SetImage(uri);
    }

    public void SetEventAsLive(bool isLive)
    {
        model.isLive = isLive;

        if (liveTag != null)
            liveTag.gameObject.SetActive(isLive);

        if (eventDateText != null && !isEventCardModal)
            eventDateText.gameObject.SetActive(!isLive);

        if (eventDateTextOnFocus != null)
            eventDateTextOnFocus.gameObject.SetActive(!isLive);

        if (jumpinButton != null)
            jumpinButton.gameObject.SetActive(isEventCardModal || isLive);

        if (secondaryJumpinButton != null)
            secondaryJumpinButton.gameObject.SetActive(isEventCardModal || isLive);

        if (subscribeEventButton != null)
            subscribeEventButton.gameObject.SetActive(!isLive && !model.eventFromAPIInfo.attending);

        if (unsubscribeEventButton != null)
            unsubscribeEventButton.gameObject.SetActive(!isLive && model.eventFromAPIInfo.attending);

        if (eventStartedInTitleForLive)
            eventStartedInTitleForLive.gameObject.SetActive(isLive);

        if (eventStartedInTitleForLiveOnFocus)
            eventStartedInTitleForLiveOnFocus.gameObject.SetActive(isLive);

        if (eventStartedInTitleForNotLive)
            eventStartedInTitleForNotLive.gameObject.SetActive(!isLive);

        if (subscribedUsersTitleForLive != null)
            subscribedUsersTitleForLive.gameObject.SetActive(isLive);

        if (subscribedUsersTitleForNotLive != null)
            subscribedUsersTitleForNotLive.gameObject.SetActive(!isLive);

        if(goingUsersGameObject != null)
            goingUsersGameObject.SetActive(!isLive);
    }

    public void SetNumberOfUsers(int newNumberOfUsers)
    {
        model.numberOfUsers = newNumberOfUsers;

        if (numberOfUsersText == null)
            return;

        numberOfUsersText.text = FormatNumber(newNumberOfUsers);
        numberOfUsersContainer.SetActive(model.isLive && newNumberOfUsers > 0);
    }

    public void SetLiveTagText(string newText)
    {
        model.liveTagText = newText;

        if (liveTag != null)
            liveTag.SetText(newText);
    }

    public void SetEventDate(string newDate)
    {
        model.eventDateText = newDate;

        if (eventDateText != null)
            eventDateText.text = newDate;

        if (eventDateTextOnFocus != null)
            eventDateTextOnFocus.text = newDate;
    }

    public void SetEventName(string newText)
    {
        model.eventName = newText;

        string wrappedText = newText.Substring(0, Math.Min(EVENT_TITLE_LENGTH_LIMIT, newText.Length));

        if (eventNameText != null)
            eventNameText.text = wrappedText;

        if (eventNameTextOnFocus != null)
            eventNameTextOnFocus.text = wrappedText;
    }

    public void SetEventDescription(string newText)
    {
        model.eventDescription = newText;

        if (eventDescText == null)
            return;

        eventDescText.text = string.IsNullOrEmpty(newText) ? NO_DESCRIPTION_TEXT : newText;
    }

    public void SetEventStartedIn(string newText)
    {
        model.eventStartedIn = newText;

        if (eventStartedInText == null)
            return;

        eventStartedInText.text = newText;
    }

    public void SetEventStartsInFromTo(string newText)
    {
        model.eventStartsInFromTo = newText;

        if (eventStartsInFromToText == null)
            return;

        eventStartsInFromToText.text = newText;
    }

    public void SetEventOrganizer(string newText)
    {
        model.eventOrganizer = newText;

        if (eventOrganizerText == null)
            return;

        eventOrganizerText.text = newText;
    }

    public void SetEventPlace(string newText, bool isWorld)
    {
        model.eventPlace = newText;

        if (eventPlaceText != null)
            eventPlaceText.text = newText;

        if (placeIcon != null)
            placeIcon.SetActive(!isWorld);

        if (worldIcon != null)
            worldIcon.SetActive(isWorld);
    }

    public void SetSubscribersUsers(int newNumberOfUsers)
    {
        model.subscribedUsers = newNumberOfUsers;
        if (subscribedUsersText == null)
            return;

        if (!isEventCardModal)
        {
            subscribedUsersText.text = $"{newNumberOfUsers.ToString()} interested";
            subscribedUsersText.gameObject.SetActive(!model.isLive);
        }
        else
            subscribedUsersText.text = newNumberOfUsers > 0 ? string.Format(USERS_CONFIRMED_MESSAGE, newNumberOfUsers) : NOBODY_CONFIRMED_MESSAGE;
    }

    public void SetCoords(Vector2Int newCoords, string worldAddress)
    {
        model.coords = newCoords;
        model.worldAddress = worldAddress;

        if (secondaryJumpinButton == null || !isEventCardModal)
            return;

        secondaryJumpinButton.SetText(string.IsNullOrEmpty(worldAddress) ? $"{newCoords.x},{newCoords.y}" : worldAddress);
    }

    public void SetLoadingIndicatorVisible(bool isVisible)
    {
        imageContainer.SetActive(!isVisible);
        eventInfoContainer.SetActive(!isVisible);
        loadingSpinner.SetActive(isVisible);
    }

    internal void RebuildCardLayouts()
    {
        if (contentVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(contentVerticalLayout.transform as RectTransform);

        if (infoVerticalLayout != null)
            Utils.ForceRebuildLayoutImmediate(infoVerticalLayout.transform as RectTransform);

        if (timeAndPlayersHorizontalLayout != null)
            Utils.ForceRebuildLayoutImmediate(timeAndPlayersHorizontalLayout.transform as RectTransform);

        if (numberOfUsersContainer != null)
            Utils.ForceRebuildLayoutImmediate(numberOfUsersContainer.transform as RectTransform);
    }

    internal void CloseModal() { Hide(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseModal(); }

    private static string FormatNumber(int num)
    {
        if (num < 1000)
            return num.ToString();

        float divided = num / 1000.0f;
        divided = (int)(divided * 100) / 100f;
        return $"{divided:F2}k";
    }

    private void ResetScrollPosition()
    {
        if (scroll == null)
            return;

        scroll.verticalNormalizedPosition = 1;
    }
}
