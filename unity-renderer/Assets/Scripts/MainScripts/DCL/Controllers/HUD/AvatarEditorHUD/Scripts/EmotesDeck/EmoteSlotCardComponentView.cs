using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IEmoteSlotCardComponentView
{
    /// <summary>
    /// Event that will be triggered when the card is clicked.
    /// </summary>
    Button.ButtonClickedEvent onClick { get; }

    /// <summary>
    /// Set the emote id in the card.
    /// </summary>
    /// <param name="id">New emote id.</param>
    void SetEmoteId(string id);

    /// <summary>
    /// Set the emote picture directly from a sprite.
    /// </summary>
    /// <param name="sprite">Emote picture (sprite).</param>
    void SetEmotePicture(Sprite sprite);

    /// <summary>
    /// Set the emote picture from an uri.
    /// </summary>
    /// <param name="uri">Emote picture (url).</param>
    void SetEmotePicture(string uri);

    /// <summary>
    /// Set the emote as selected or not.
    /// </summary>
    /// <param name="isSelected">True for select it.</param>
    void SetEmoteAsSelected(bool isSelected);

    /// <summary>
    /// Set the slot number in the card.
    /// </summary>
    /// <param name="slotNumber">Slot number of the card.</param>
    void SetSlotNumber(int slotNumber);
}

public class EmoteSlotCardComponentView : BaseComponentView, IEmoteSlotCardComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView emoteImage;
    [SerializeField] internal TMP_Text slotNumberText;
    [SerializeField] internal ButtonComponentView mainButton;
    [SerializeField] internal Image defaultBackgroundImage;
    [SerializeField] internal Image selectedBackgroundImage;

    [Header("Configuration")]
    [SerializeField] internal Sprite defaultEmotePicture;
    [SerializeField] internal Sprite nonEmoteAssignedPicture;
    [SerializeField] internal Color defaultBackgroundColor;
    [SerializeField] internal Color selectedBackgroundColor;
    [SerializeField] internal Color defaultTextColor;
    [SerializeField] internal Color selectedTextColor;
    [SerializeField] internal EmoteSlotCardComponentModel model;

    public Button.ButtonClickedEvent onClick => mainButton?.onClick;

    public override void Awake()
    {
        base.Awake();

        if (emoteImage != null)
            emoteImage.OnLoaded += OnEmoteImageLoaded;
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (EmoteSlotCardComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetEmoteId(model.emoteId);

        if (model.pictureSprite != null)
            SetEmotePicture(model.pictureSprite);
        else if (!string.IsNullOrEmpty(model.pictureUri))
            SetEmotePicture(model.pictureUri);
        else
            OnEmoteImageLoaded(null);

        SetEmoteAsSelected(model.isSelected);
        SetSlotNumber(model.slotNumber);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (!model.isSelected)
        {
            SetSelectedVisualsForHovering(true);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (!model.isSelected)
        {
            SetSelectedVisualsForHovering(false);
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        if (emoteImage != null)
        {
            emoteImage.OnLoaded -= OnEmoteImageLoaded;
            emoteImage.Dispose();
        }
    }

    public void SetEmoteId(string id) 
    { 
        model.emoteId = id;

        if (nonEmoteAssignedPicture != null)
        {
            if (string.IsNullOrEmpty(id))
                SetEmotePicture(nonEmoteAssignedPicture);
        }
    }

    public void SetEmotePicture(Sprite sprite)
    {
        if (sprite == null && defaultEmotePicture != null)
            sprite = defaultEmotePicture;

        model.pictureSprite = sprite;

        if (emoteImage == null)
            return;

        emoteImage.SetImage(sprite);
    }

    public void SetEmotePicture(string uri)
    {
        if (string.IsNullOrEmpty(uri) && defaultEmotePicture != null)
        {
            SetEmotePicture(defaultEmotePicture);
            return;
        }

        model.pictureUri = uri;

        if (!Application.isPlaying)
            return;

        if (emoteImage == null)
            return;

        emoteImage.SetImage(uri);
    }

    public void SetEmoteAsSelected(bool isSelected)
    {
        model.isSelected = isSelected;

        SetSelectedVisualsForClicking(isSelected);
    }

    public void SetSlotNumber(int slotNumber)
    {
        model.slotNumber = slotNumber;

        if (slotNumberText == null)
            return;

        slotNumberText.text = slotNumber.ToString();
    }

    internal void OnEmoteImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            SetEmotePicture(sprite);
        else
            SetEmotePicture(sprite: null);
    }

    internal void SetSelectedVisualsForClicking(bool isSelected)
    {
        if (defaultBackgroundImage != null)
        {
            defaultBackgroundImage.gameObject.SetActive(!isSelected);
            defaultBackgroundImage.color = defaultBackgroundColor;
        }

        if (selectedBackgroundImage != null)
        {
            selectedBackgroundImage.gameObject.SetActive(isSelected);
            selectedBackgroundImage.color = selectedBackgroundColor;
        }

        if (slotNumberText)
            slotNumberText.color = isSelected ? selectedTextColor : defaultTextColor;
    }

    internal void SetSelectedVisualsForHovering(bool isSelected)
    {
        if (defaultBackgroundImage != null)
            defaultBackgroundImage.color = isSelected ? selectedBackgroundColor : defaultBackgroundColor;

        if (slotNumberText)
            slotNumberText.color = isSelected ? selectedTextColor : defaultTextColor;
    }
}
