using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPlaceCardComponentView
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
    /// Fill the model and updates the place card with this data.
    /// </summary>
    /// <param name="model">Data to configure the place card.</param>
    void Configure(PlaceCardComponentModel model);

    /// <summary>
    /// Set the place picture directly from a sprite.
    /// </summary>
    /// <param name="sprite">Place picture (sprite).</param>
    void SetPlacePicture(Sprite sprite);

    /// <summary>
    /// Set the place picture from a 2D texture.
    /// </summary>
    /// <param name="texture">Place picture (texture).</param>
    void SetPlacePicture(Texture2D texture);

    /// <summary>
    /// Set the place picture from an uri.
    /// </summary>
    /// <param name="uri">Place picture (url).</param>
    void SetPlacePicture(string uri);

    /// <summary>
    /// Set the place name in the card.
    /// </summary>
    /// <param name="newText">New place name.</param>
    void SetPlaceName(string newText);

    /// <summary>
    /// Set the place description in the card.
    /// </summary>
    /// <param name="newText">New place description.</param>
    void SetPlaceDescription(string newText);

    /// <summary>
    /// Set the place organizer in the card.
    /// </summary>
    /// <param name="newText">New place organizer.</param>
    void SetPlaceAuthor(string newText);

    /// <summary>
    /// Set the the number of users in the place.
    /// </summary>
    /// <param name="newNumberOfUsers">Number of users.</param>
    void SetNumberOfUsers(int newNumberOfUsers);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator and hiding the card info.</param>
    void SetLoadingIndicatorVisible(bool isVisible);

    /// <summary>
    /// Set the configuration of the JumpIn button.
    /// </summary>
    /// <param name="jumpInConfig">JumpIn configuration.</param>
    void SetJumpInConfiguration(JumpInConfig jumpInConfig);
}

public class PlaceCardComponentView : BaseComponentView, IPlaceCardComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView placeImage;
    [SerializeField] internal TMP_Text placeNameText;
    [SerializeField] internal TMP_Text placeDescText;
    [SerializeField] internal TMP_Text placeAuthorText;
    [SerializeField] internal TMP_Text numberOfUsersText;
    [SerializeField] internal TMP_Text coordsText;
    [SerializeField] internal ButtonComponentView infoButton;
    [SerializeField] internal ButtonComponentView jumpinButton;
    [SerializeField] internal GameObject imageContainer;
    [SerializeField] internal GameObject placeInfoContainer;
    [SerializeField] internal GameObject loadingSpinner;

    [Header("Configuration")]
    [SerializeField] internal bool isPlaceCardModal = false;
    [SerializeField] internal PlaceCardComponentModel model;

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

    public override void PostInitialization()
    {
        if (placeImage != null)
            placeImage.OnLoaded += OnPlaceImageLoaded;

        Configure(model);
    }

    public void Configure(PlaceCardComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        if (model.placePictureSprite != null)
            SetPlacePicture(model.placePictureSprite);
        else if (model.placePictureTexture != null)
            SetPlacePicture(model.placePictureTexture);
        else if (!string.IsNullOrEmpty(model.placePictureUri))
            SetPlacePicture(model.placePictureUri);
        else
            SetPlacePicture(sprite: null);

        SetPlaceName(model.placeName);
        SetPlaceDescription(model.placeDescription);
        SetPlaceAuthor(model.placeAuthor);
        SetNumberOfUsers(model.numberOfUsers);
        SetJumpInConfiguration(model.jumpInConfiguration);
        onJumpInClick = model.onJumpInClick;
        onInfoClick = model.onInfoClick;
    }

    public override void Dispose()
    {
        base.Dispose();

        if (placeImage != null)
            placeImage.OnLoaded -= OnPlaceImageLoaded;

        if (infoButton != null)
            infoButton.onClick.RemoveAllListeners();

        if (jumpinButton != null)
            jumpinButton.onClick.RemoveAllListeners();
    }

    public void SetPlacePicture(Sprite sprite)
    {
        model.placePictureSprite = sprite;

        if (placeImage == null)
            return;

        placeImage.SetImage(sprite);
    }

    public void SetPlacePicture(Texture2D texture)
    {
        model.placePictureTexture = texture;

        if (!Application.isPlaying)
            return;

        if (placeImage == null)
            return;

        placeImage.SetImage(texture);
    }

    public void SetPlacePicture(string uri)
    {
        model.placePictureUri = uri;

        if (!Application.isPlaying)
            return;

        if (placeImage == null)
            return;

        placeImage.SetImage(uri);
    }

    public void SetPlaceName(string newText)
    {
        model.placeName = newText;

        if (placeNameText == null)
            return;

        placeNameText.text = newText;
    }

    public void SetPlaceDescription(string newText)
    {
        model.placeDescription = newText;

        if (placeDescText == null)
            return;

        placeDescText.text = newText;
    }

    public void SetPlaceAuthor(string newText)
    {
        model.placeAuthor = newText;

        if (placeAuthorText == null)
            return;

        placeAuthorText.text = newText;
    }

    public void SetNumberOfUsers(int newNumberOfUsers)
    {
        model.numberOfUsers = newNumberOfUsers;

        if (numberOfUsersText == null)
            return;

        numberOfUsersText.text = newNumberOfUsers.ToString();
    }

    public void SetLoadingIndicatorVisible(bool isVisible)
    {
        imageContainer.SetActive(!isVisible);
        placeInfoContainer.SetActive(!isVisible);
        loadingSpinner.SetActive(isVisible);
    }

    public void SetJumpInConfiguration(JumpInConfig jumpInConfig)
    {
        if (jumpinButton != null)
        {
            jumpinButton.GetComponent<JumpInAction>().coords = jumpInConfig.coords;
            jumpinButton.GetComponent<JumpInAction>().serverName = jumpInConfig.serverName;
            jumpinButton.GetComponent<JumpInAction>().layerName = jumpInConfig.layerName;
        }

        if (coordsText != null)
            coordsText.text = $"{jumpInConfig.coords[0]},{jumpInConfig.coords[1]}";
    }

    private void OnPlaceImageLoaded(Sprite sprite) { SetPlacePicture(sprite); }
}