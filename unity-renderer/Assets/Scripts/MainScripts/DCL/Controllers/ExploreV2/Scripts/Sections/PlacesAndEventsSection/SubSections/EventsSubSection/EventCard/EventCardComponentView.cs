using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IEventCardComponentView
{
    /// <summary>
    /// Event that will be triggered when the jumpIn button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onJumpInClick { get; set; }

    /// <summary>
    /// Event that will be triggered when the info button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onInfoClick { get; set; }

    /// <summary>
    /// Event that will be triggered when the subscribe event button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onSubscribeClick { get; set; }

    /// <summary>
    /// Event that will be triggered when the unsubscribe event button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onUnsubscribeClick { get; set; }

    /// <summary>
    /// Fill the model and updates the event card with this data.
    /// </summary>
    /// <param name="model">Data to configure the event card.</param>
    void Configure(EventCardComponentModel model);

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
    void SetEventPlace(string newText);

    /// <summary>
    /// Set the the number of users subscribed to the event.
    /// </summary>
    /// <param name="newNumberOfUsers">Number of users subscribed.</param>
    void SetSubscribersUsers(int newNumberOfUsers);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator and hiding the card info.</param>
    void SetLoadingIndicatorVisible(bool isVisible);
}

public class EventCardComponentView : BaseComponentView, IEventCardComponentView
{
    internal static readonly int ON_FOCUS_CARD_COMPONENT_BOOL = Animator.StringToHash("OnFocus");

    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView eventImage;
    [SerializeField] internal TagComponentView liveTag;
    [SerializeField] internal TMP_Text eventDateText;
    [SerializeField] internal TMP_Text eventNameText;
    [SerializeField] internal TMP_Text eventDescText;
    [SerializeField] internal TMP_Text eventStartedInTitleForLive;
    [SerializeField] internal TMP_Text eventStartedInTitleForNotLive;
    [SerializeField] internal TMP_Text eventStartedInText;
    [SerializeField] internal TMP_Text eventStartsInFromToText;
    [SerializeField] internal TMP_Text eventOrganizerText;
    [SerializeField] internal TMP_Text eventPlaceText;
    [SerializeField] internal TMP_Text subscribedUsersTitleForLive;
    [SerializeField] internal TMP_Text subscribedUsersTitleForNotLive;
    [SerializeField] internal TMP_Text subscribedUsersText;
    [SerializeField] internal ButtonComponentView infoButton;
    [SerializeField] internal ButtonComponentView jumpinButton;
    [SerializeField] internal ButtonComponentView subscribeEventButton;
    [SerializeField] internal ButtonComponentView unsubscribeEventButton;
    [SerializeField] internal GameObject imageContainer;
    [SerializeField] internal GameObject eventInfoContainer;
    [SerializeField] internal GameObject loadingSpinner;
    [SerializeField] internal Animator cardAnimator;

    [Header("Configuration")]
    [SerializeField] internal bool isEventCardModal = false;
    [SerializeField] internal EventCardComponentModel model;

    public Button.ButtonClickedEvent onJumpInClick
    {
        get
        {
            if (jumpinButton == null)
                return null;

            return jumpinButton.onClick;
        }
        set
        {
            model.onJumpInClick = value;

            if (jumpinButton != null)
                jumpinButton.onClick = value;
        }
    }

    public Button.ButtonClickedEvent onInfoClick
    {
        get
        {
            if (infoButton == null)
                return null;

            return infoButton.onClick;
        }
        set
        {
            model.onInfoClick = value;

            if (infoButton != null)
                infoButton.onClick = value;
        }
    }

    public Button.ButtonClickedEvent onSubscribeClick
    {
        get
        {
            if (subscribeEventButton == null)
                return null;

            return subscribeEventButton.onClick;
        }
        set
        {
            model.onSubscribeClick = value;

            if (subscribeEventButton != null)
                subscribeEventButton.onClick = value;
        }
    }

    public Button.ButtonClickedEvent onUnsubscribeClick
    {
        get
        {
            if (unsubscribeEventButton == null)
                return null;

            return unsubscribeEventButton.onClick;
        }
        set
        {
            model.onUnsubscribeClick = value;

            if (unsubscribeEventButton != null)
                unsubscribeEventButton.onClick = value;
        }
    }

    public override void PostInitialization()
    {
        if (eventImage != null)
            eventImage.OnLoaded += OnEventImageLoaded;

        Configure(model);
    }

    public void Configure(EventCardComponentModel model)
    {
        this.model = model;
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
        SetLiveTagText(model.liveTagText);
        SetEventDate(model.eventDateText);
        SetEventName(model.eventName);
        SetEventDescription(model.eventDescription);
        SetEventStartedIn(model.eventStartedIn);
        SetEventStartsInFromTo(model.eventStartsInFromTo);
        SetEventOrganizer(model.eventOrganizer);
        SetEventPlace(model.eventPlace);
        SetSubscribersUsers(model.subscribedUsers);
        onJumpInClick = model.onJumpInClick;
        onInfoClick = model.onInfoClick;
        onSubscribeClick = model.onSubscribeClick;
        onUnsubscribeClick = model.onUnsubscribeClick;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (cardAnimator != null)
            cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, true);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (cardAnimator != null)
            cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, false);
    }

    public override void Dispose()
    {
        base.Dispose();

        if (eventImage != null)
            eventImage.OnLoaded -= OnEventImageLoaded;

        if (infoButton != null)
            infoButton.onClick.RemoveAllListeners();

        if (subscribeEventButton != null)
            subscribeEventButton.onClick.RemoveAllListeners();

        if (unsubscribeEventButton != null)
            unsubscribeEventButton.onClick.RemoveAllListeners();

        if (jumpinButton != null)
            jumpinButton.onClick.RemoveAllListeners();
    }

    public void SetEventPicture(Sprite sprite)
    {
        model.eventPictureSprite = sprite;

        if (eventImage == null)
            return;

        eventImage.SetImage(sprite);
    }

    public void SetEventPicture(Texture2D texture)
    {
        model.eventPictureTexture = texture;

        if (!Application.isPlaying)
            return;

        if (eventImage == null)
            return;

        eventImage.SetImage(texture);
    }

    public void SetEventPicture(string uri)
    {
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

        if (eventDateText != null)
            eventDateText.gameObject.SetActive(!isLive);

        if (jumpinButton != null)
            jumpinButton.gameObject.SetActive(isLive);

        if (subscribeEventButton != null)
            subscribeEventButton.gameObject.SetActive(!isLive && !model.isSubscribed);

        if (unsubscribeEventButton != null)
            unsubscribeEventButton.gameObject.SetActive(!isLive && model.isSubscribed);

        if (eventStartedInTitleForLive)
            eventStartedInTitleForLive.gameObject.SetActive(isLive);

        if (eventStartedInTitleForNotLive)
            eventStartedInTitleForNotLive.gameObject.SetActive(!isLive);

        if (subscribedUsersTitleForLive != null)
            subscribedUsersTitleForLive.gameObject.SetActive(isLive);

        if (subscribedUsersTitleForNotLive != null)
            subscribedUsersTitleForNotLive.gameObject.SetActive(!isLive);
    }

    public void SetLiveTagText(string newText)
    {
        model.liveTagText = newText;

        if (liveTag == null)
            return;

        liveTag.SetText(newText);
    }

    public void SetEventDate(string newDate)
    {
        model.eventDateText = newDate;

        if (eventDateText == null)
            return;

        eventDateText.text = newDate;
    }

    public void SetEventName(string newText)
    {
        model.eventName = newText;

        if (eventNameText == null)
            return;

        eventNameText.text = newText;
    }

    public void SetEventDescription(string newText)
    {
        model.eventDescription = newText;

        if (eventDescText == null)
            return;

        eventDescText.text = newText;
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

    public void SetEventPlace(string newText)
    {
        model.eventPlace = newText;

        if (eventPlaceText == null)
            return;

        eventPlaceText.text = newText;
    }

    public void SetSubscribersUsers(int newNumberOfUsers)
    {
        model.subscribedUsers = newNumberOfUsers;

        if (subscribedUsersText == null)
            return;

        if (!isEventCardModal)
        {
            subscribedUsersText.text = newNumberOfUsers.ToString();
        }
        else
        {
            if (newNumberOfUsers > 0)
                subscribedUsersText.text = $"{newNumberOfUsers} confirmed";
            else
                subscribedUsersText.text = "Nobody confirmed yet";
        }
    }

    public void SetLoadingIndicatorVisible(bool isVisible)
    {
        imageContainer.SetActive(!isVisible);
        eventInfoContainer.SetActive(!isVisible);
        loadingSpinner.SetActive(isVisible);
    }

    private void OnEventImageLoaded(Sprite sprite) { SetEventPicture(sprite); }
}