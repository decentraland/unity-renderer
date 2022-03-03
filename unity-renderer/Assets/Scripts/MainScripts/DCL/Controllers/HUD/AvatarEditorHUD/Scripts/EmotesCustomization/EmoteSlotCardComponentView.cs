using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emotes
{
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
        /// Set the emote name in the card.
        /// </summary>
        /// <param name="name">New emote name.</param>
        void SetEmoteName(string name);

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
        /// <param name="slotNumber">Slot number of the card (between 0 and 9).</param>
        void SetSlotNumber(int slotNumber);

        /// <summary>
        /// Set the emote separator as active or not.
        /// </summary>
        /// <param name="isActive">True for activating it.</param>
        void SetSeparatorActive(bool isActive);

        /// <summary>
        /// Set the type of rarity in the card.
        /// </summary>
        /// <param name="rarity">New rarity.</param>
        void SetRarity(string rarity);
    }

    public class EmoteSlotCardComponentView : BaseComponentView, IEmoteSlotCardComponentView, IComponentModelConfig
    {
        internal const int SLOT_VIEWER_ROTATION_ANGLE = 36;
        internal const string EMPTY_SLOT_TEXT = "None";

        [Header("Prefab References")]
        [SerializeField] internal ImageComponentView emoteImage;
        [SerializeField] internal Image nonEmoteImage;
        [SerializeField] internal TMP_Text emoteNameText;
        [SerializeField] internal TMP_Text slotNumberText;
        [SerializeField] internal Image slotViewerImage;
        [SerializeField] internal ButtonComponentView mainButton;
        [SerializeField] internal Image defaultBackgroundImage;
        [SerializeField] internal Image selectedBackgroundImage;
        [SerializeField] internal GameObject separatorGO;
        [SerializeField] internal Image rarityMark;

        [Header("Configuration")]
        [SerializeField] internal Sprite defaultEmotePicture;
        [SerializeField] internal Color defaultBackgroundColor;
        [SerializeField] internal Color selectedBackgroundColor;
        [SerializeField] internal Color defaultSlotNumberColor;
        [SerializeField] internal Color selectedSlotNumberColor;
        [SerializeField] internal Color defaultEmoteNameColor;
        [SerializeField] internal Color selectedEmoteNameColor;
        [SerializeField] internal List<EmoteRarity> rarityColors;
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

            if (model.pictureSprite != null)
                SetEmotePicture(model.pictureSprite);
            else if (!string.IsNullOrEmpty(model.pictureUri))
                SetEmotePicture(model.pictureUri);
            else
                OnEmoteImageLoaded(null);

            SetEmoteId(model.emoteId);
            SetEmoteName(model.emoteName);
            SetEmoteAsSelected(model.isSelected);
            SetSlotNumber(model.slotNumber);
            SetSeparatorActive(model.hasSeparator);
            SetRarity(model.rarity);
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

            if (nonEmoteImage != null)
                nonEmoteImage.gameObject.SetActive(string.IsNullOrEmpty(id));

            if (emoteImage != null)
                emoteImage.gameObject.SetActive(!string.IsNullOrEmpty(id));
        }

        public void SetEmoteName(string name)
        {
            model.emoteName = name;

            if (emoteNameText == null)
                return;

            emoteNameText.text = string.IsNullOrEmpty(name) ? EMPTY_SLOT_TEXT : name;
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
            model.slotNumber = Mathf.Clamp(slotNumber, 0, 9);

            if (slotNumberText != null)
                slotNumberText.text = model.slotNumber.ToString();

            if (slotViewerImage != null)
                slotViewerImage.transform.rotation = Quaternion.Euler(0, 0, -model.slotNumber * SLOT_VIEWER_ROTATION_ANGLE);
        }

        public void SetSeparatorActive(bool isActive)
        {
            model.hasSeparator = isActive;

            if (separatorGO == null)
                return;

            separatorGO.SetActive(isActive);
        }

        public void SetRarity(string rarity)
        {
            model.rarity = rarity;

            if (rarityMark == null)
                return;

            EmoteRarity emoteRarity = rarityColors.FirstOrDefault(x => x.rarity == rarity);
            rarityMark.gameObject.SetActive(emoteRarity != null);
            rarityMark.color = emoteRarity != null ? emoteRarity.markColor : Color.white;
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

            if (slotNumberText != null)
                slotNumberText.color = isSelected ? selectedSlotNumberColor : defaultSlotNumberColor;

            if (emoteNameText != null)
                emoteNameText.color = isSelected ? selectedEmoteNameColor : defaultEmoteNameColor;

            if (slotViewerImage != null)
                slotViewerImage.gameObject.SetActive(isSelected);
        }

        internal void SetSelectedVisualsForHovering(bool isSelected)
        {
            if (defaultBackgroundImage != null)
                defaultBackgroundImage.color = isSelected ? selectedBackgroundColor : defaultBackgroundColor;

            if (slotNumberText != null)
                slotNumberText.color = isSelected ? selectedSlotNumberColor : defaultSlotNumberColor;

            if (emoteNameText != null)
                emoteNameText.color = isSelected ? selectedEmoteNameColor : defaultEmoteNameColor;

            if (slotViewerImage != null)
                slotViewerImage.gameObject.SetActive(isSelected);
        }
    }
}