using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emotes
{
    public interface IEmoteCardComponentView
    {
        /// <summary>
        /// Event that will be triggered when the main button is clicked.
        /// </summary>
        Button.ButtonClickedEvent onMainClick { get; }

        /// <summary>
        /// Event that will be triggered when the info button is clicked.
        /// </summary>
        Button.ButtonClickedEvent onInfoClick { get; }

        /// <summary>
        /// Event that will be triggered when the equip button is clicked.
        /// </summary>
        Button.ButtonClickedEvent onEquipClick { get; }

        /// <summary>
        /// Event that will be triggered when the unequip button is clicked.
        /// </summary>
        Button.ButtonClickedEvent onUnequipClick { get; }

        /// <summary>
        /// It will be triggered when an emote card is selected.
        /// </summary>
        event Action<string> onEmoteSelected;

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
        /// Set the emote description in the card.
        /// </summary>
        /// <param name="description">New emote description.</param>
        void SetEmoteDescription(string description);

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
        /// Set the emote as assigned in selected slot or not.
        /// </summary>
        /// <param name="isAssigned">True for select it.</param>
        void SetEmoteAsAssignedInSelectedSlot(bool isAssigned);

        /// <summary>
        /// Assign a slot number to the emote.
        /// </summary>
        /// <param name="slotNumber">Slot number to assign.</param>
        void AssignSlot(int slotNumber);

        /// <summary>
        /// Set the emote as selected.
        /// </summary>
        /// <param name="isSelected">True for selecting it.</param>
        void SetEmoteAsSelected(bool isSelected);

        /// <summary>
        /// Set the type of rarity in the card.
        /// </summary>
        /// <param name="rarity">New rarity.</param>
        void SetRarity(string rarity);

        /// <summary>
        /// Set the emote as L2 or not.
        /// </summary>
        /// <param name="isInL2">True for set it as L2.</param>
        void SetIsInL2(bool isInL2);
    }

    public class EmoteCardComponentView : BaseComponentView, IEmoteCardComponentView, IComponentModelConfig
    {
        internal static readonly int ON_SELECTED_CARD_COMPONENT_BOOL = Animator.StringToHash("OnSelected");

        [Header("Prefab References")]
        [SerializeField] internal ImageComponentView emoteImage;
        [SerializeField] internal GameObject emoteNameContainer;
        [SerializeField] internal TMP_Text emoteNameText;
        [SerializeField] internal TMP_Text assignedSlotNumberText;
        [SerializeField] internal ImageComponentView assignedInCurrentSlotMarkImage;
        [SerializeField] internal ButtonComponentView mainButton;
        [SerializeField] internal ButtonComponentView infoButton;
        [SerializeField] internal ButtonComponentView equipButton;
        [SerializeField] internal ButtonComponentView unequipButton;
        [SerializeField] internal GameObject cardSelectionFrame;
        [SerializeField] internal Animator selectionAnimator;
        [SerializeField] internal Image rarityMark;
        [SerializeField] internal Transform emoteInfoAnchor;


        [Header("Configuration")]
        [SerializeField] internal Sprite defaultEmotePicture;
        [SerializeField] internal Sprite nonEmoteAssignedPicture;
        [SerializeField] internal List<EmoteRarity> rarityColors;
        [SerializeField] internal EmoteCardComponentModel model;

        public Button.ButtonClickedEvent onMainClick => mainButton?.onClick;
        public Button.ButtonClickedEvent onInfoClick => infoButton?.onClick;
        public Button.ButtonClickedEvent onEquipClick => equipButton?.onClick;
        public Button.ButtonClickedEvent onUnequipClick => unequipButton?.onClick;
        
        public event Action<string> onEmoteSelected;

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
            SetEmoteName(model.name);
            SetEmoteDescription(model.description);

            if (model.pictureSprite != null)
                SetEmotePicture(model.pictureSprite);
            else if (!string.IsNullOrEmpty(model.pictureUri))
                SetEmotePicture(model.pictureUri);
            else
                OnEmoteImageLoaded(null);

            SetEmoteAsAssignedInSelectedSlot(model.isAssignedInSelectedSlot);
            AssignSlot(model.assignedSlot);
            SetEmoteAsSelected(model.isSelected);
            SetRarity(model.rarity);
            SetIsInL2(model.isInL2);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            RefreshControl();
        }

        public override void OnFocus()
        {
            base.OnFocus();

            if (emoteNameContainer != null)
                emoteNameContainer.SetActive(true);

            SetEmoteAsSelected(true);
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();

            if (emoteNameContainer != null)
                emoteNameContainer.SetActive(false);

            SetEmoteAsSelected(false);
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
            model.id = id;

            if (string.IsNullOrEmpty(id))
            {
                if (nonEmoteAssignedPicture != null)
                    SetEmotePicture(nonEmoteAssignedPicture);
            }
        }

        public void SetEmoteName(string name)
        {
            model.name = name;

            if (emoteNameText == null)
                return;

            emoteNameText.text = name;
        }

        public void SetEmoteDescription(string description)
        {
            model.description = description;
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

        public void SetEmoteAsAssignedInSelectedSlot(bool isAssigned)
        {
            model.isAssignedInSelectedSlot = isAssigned;

            RefreshVisualCardStatus();
        }

        public void AssignSlot(int slotNumber)
        {
            model.assignedSlot = slotNumber;

            if (assignedSlotNumberText == null)
                return;

            assignedSlotNumberText.text = slotNumber.ToString();

            RefreshVisualCardStatus();
        }

        public void SetEmoteAsSelected(bool isSelected)
        {
            model.isSelected = isSelected;

            cardSelectionFrame.SetActive(isSelected);

            if (selectionAnimator != null)
                selectionAnimator.SetBool(ON_SELECTED_CARD_COMPONENT_BOOL, isSelected);

            RefreshVisualCardStatus();

            if (isSelected)
                onEmoteSelected?.Invoke(model.id);
            else
                onEmoteSelected?.Invoke(null);
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

        public void SetIsInL2(bool isInL2)
        {
            model.isInL2 = isInL2;
        }

        internal void RefreshVisualCardStatus()
        {
            RefreshAssignedSlotTextVisibility();
            RefreshAssignedInCurrentSlotMarkVisibility();
            RefreshCardButtonsVisibility();
        }

        internal void RefreshAssignedSlotTextVisibility()
        {
            if (assignedSlotNumberText == null)
                return;

            assignedSlotNumberText.gameObject.SetActive(
                !model.isAssignedInSelectedSlot &&
                !model.isSelected &&
                model.assignedSlot >= 0);
        }

        internal void RefreshAssignedInCurrentSlotMarkVisibility()
        {
            if (assignedInCurrentSlotMarkImage == null)
                return;

            assignedInCurrentSlotMarkImage.gameObject.SetActive(model.isAssignedInSelectedSlot && !model.isSelected);
        }

        internal void RefreshCardButtonsVisibility()
        {
            if (infoButton != null)
                infoButton.gameObject.SetActive(model.isSelected);

            if (equipButton != null)
                equipButton.gameObject.SetActive(model.isSelected && !model.isAssignedInSelectedSlot);

            if (unequipButton != null)
                unequipButton.gameObject.SetActive(model.isSelected && model.isAssignedInSelectedSlot);
        }

        internal void OnEmoteImageLoaded(Sprite sprite)
        {
            if (sprite != null)
                SetEmotePicture(sprite);
            else
                SetEmotePicture(sprite: null);
        }
    }
}