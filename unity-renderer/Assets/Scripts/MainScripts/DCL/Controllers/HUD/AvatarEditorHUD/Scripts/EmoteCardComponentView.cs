using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IEmoteCardComponentView
{
    /// <summary>
    /// Event that will be triggered when the equip button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onEquipClick { get; }

    /// <summary>
    /// Event that will be triggered when the open details button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onOpenDetailsClick { get; }

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
    /// Set the emote as equipped or not.
    /// </summary>
    /// <param name="isEquipped">True for equip it.</param>
    void SetEmoteAsEquipped(bool isEquipped);

    /// <summary>
    /// Set the emote as selected or not.
    /// </summary>
    /// <param name="isSelected">True for select it.</param>
    void SetEmoteAsSelected(bool isSelected);

    /// <summary>
    /// Assign a slot number to the emote.
    /// </summary>
    /// <param name="slotNumber">Slot number to assign.</param>
    void AssignSlot(int slotNumber);
}

public class EmoteCardComponentView : BaseComponentView, IEmoteCardComponentView, IComponentModelConfig
{
    internal static readonly int ON_FOCUS_CARD_COMPONENT_BOOL = Animator.StringToHash("OnFocus");

    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView emoteImage;
    [SerializeField] internal ImageComponentView favActivatedImage;
    [SerializeField] internal ImageComponentView favDeactivatedImage;
    [SerializeField] internal TMP_Text assignedSlotNumberText;
    [SerializeField] internal ImageComponentView selectedMarkImage;
    [SerializeField] internal ButtonComponentView openDetailsButton;
    [SerializeField] internal ButtonComponentView equipButton;
    [SerializeField] internal GameObject cardSelectionFrame;
    [SerializeField] internal Animator cardAnimator;

    [Header("Configuration")]
    [SerializeField] internal Sprite defaultEmotePicture;
    [SerializeField] internal EmoteCardComponentModel model;

    public Button.ButtonClickedEvent onOpenDetailsClick => openDetailsButton?.onClick;
    public Button.ButtonClickedEvent onEquipClick => equipButton?.onClick;

    public override void Awake()
    {
        base.Awake();

        if (emoteImage != null)
            emoteImage.OnLoaded += OnEmoteImageLoaded;

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (EmoteCardComponentModel)newModel;

        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetEmoteId(model.id);

        if (model.pictureSprite != null)
            SetEmotePicture(model.pictureSprite);
        else if (!string.IsNullOrEmpty(model.pictureUri))
            SetEmotePicture(model.pictureUri);
        else
            OnEmoteImageLoaded(null);

        SetEmoteAsEquipped(model.isEquipped);
        SetEmoteAsSelected(model.isSelected);
        AssignSlot(model.assignedSlot);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(true);

        if (cardAnimator != null)
            cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, true);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (cardSelectionFrame != null)
            cardSelectionFrame.SetActive(false);

        if (cardAnimator != null)
            cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, false);
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

    public void SetEmoteId(string id) { model.id = id; }

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

    public void SetEmoteAsEquipped(bool isEquipped)
    {
        model.isEquipped = isEquipped;

        if (favActivatedImage != null)
        {
            if (isEquipped)
                favActivatedImage.Show();
            else
                favActivatedImage.Hide();
        }

        if (favDeactivatedImage != null)
        {
            if (isEquipped)
                favDeactivatedImage.Hide();
            else
                favDeactivatedImage.Show();
        }
    }

    public void SetEmoteAsSelected(bool isSelected)
    {
        model.isSelected = isSelected;

        if (selectedMarkImage != null)
            selectedMarkImage.gameObject.SetActive(isSelected);

        RefreshAssignedSlotVisibility();
    }

    public void AssignSlot(int slotNumber)
    {
        model.assignedSlot = slotNumber;

        if (assignedSlotNumberText == null)
            return;

        assignedSlotNumberText.text = slotNumber.ToString();

        RefreshAssignedSlotVisibility();
    }

    internal void RefreshAssignedSlotVisibility()
    {
        if (assignedSlotNumberText == null)
            return;

        assignedSlotNumberText.gameObject.SetActive(!model.isSelected && model.assignedSlot >= 0);
    }

    internal void OnEmoteImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            SetEmotePicture(sprite);
        else
            SetEmotePicture(sprite: null);
    }
}
