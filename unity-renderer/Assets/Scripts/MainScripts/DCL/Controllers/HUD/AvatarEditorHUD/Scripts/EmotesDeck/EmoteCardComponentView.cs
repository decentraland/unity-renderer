using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmotesDeck
{
    public interface IEmoteCardComponentView
    {
        /// <summary>
        /// It will be triggered when the emote card is selected/unselected.
        /// </summary>
        event Action<string, bool> onSelected;

        /// <summary>
        /// Event that will be triggered when the equip button is clicked.
        /// </summary>
        Button.ButtonClickedEvent onEquipClick { get; }

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
        /// <param name="isFavorite">True for set it as favorite.</param>
        void SetEmoteAsFavorite(bool isFavorite);

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
    }

    public class EmoteCardComponentView : BaseComponentView, IEmoteCardComponentView, IComponentModelConfig
    {
        internal static readonly int ON_FOCUS_CARD_COMPONENT_BOOL = Animator.StringToHash("OnFocus");

        [Header("Prefab References")]
        [SerializeField] internal ImageComponentView emoteImage;
        [SerializeField] internal ImageComponentView favActivatedImage;
        [SerializeField] internal ImageComponentView favDeactivatedImage;
        [SerializeField] internal TMP_Text assignedSlotNumberText;
        [SerializeField] internal ImageComponentView assignedInCurrentSlotMarkImage;
        [SerializeField] internal ButtonComponentView mainButton;
        [SerializeField] internal ButtonComponentView equipButton;
        [SerializeField] internal GameObject cardSelectionFrame;
        [SerializeField] internal Animator cardAnimator;

        [Header("Configuration")]
        [SerializeField] internal Sprite defaultEmotePicture;
        [SerializeField] internal Sprite nonEmoteAssignedPicture;
        [SerializeField] internal EmoteCardComponentModel model;

        public event Action<string, bool> onSelected;
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

            SetEmoteAsFavorite(model.isFavorite);
            SetEmoteAsAssignedInSelectedSlot(model.isAssignedInSelectedSlot);
            AssignSlot(model.assignedSlot);
            SetEmoteAsSelected(model.isSelected);
        }

        public override void OnFocus()
        {
            base.OnFocus();

            if (model.isSelected)
                return;

            if (cardSelectionFrame != null)
                cardSelectionFrame.SetActive(true);

            if (cardAnimator != null)
                cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, true);
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();

            if (model.isSelected)
                return;

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

        public void SetEmoteId(string id)
        {
            model.id = id;

            if (string.IsNullOrEmpty(id))
            {
                if (nonEmoteAssignedPicture != null)
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

        public void SetEmoteAsFavorite(bool isFavorite)
        {
            model.isFavorite = isFavorite;

            RefreshFavoriteMarkVisibility();
        }

        public void SetEmoteAsAssignedInSelectedSlot(bool isAssigned)
        {
            model.isAssignedInSelectedSlot = isAssigned;

            RefreshAssignedInCurrentSlotMarkVisibility();
            RefreshAssignedSlotTextVisibility();
        }

        public void AssignSlot(int slotNumber)
        {
            model.assignedSlot = slotNumber;

            if (assignedSlotNumberText == null)
                return;

            assignedSlotNumberText.text = slotNumber.ToString();

            RefreshAssignedSlotTextVisibility();
        }

        public void SetEmoteAsSelected(bool isSelected)
        {
            model.isSelected = isSelected;

            if (cardSelectionFrame != null && !isFocused)
                cardSelectionFrame.SetActive(isSelected);

            if (cardAnimator != null && !isFocused)
                cardAnimator.SetBool(ON_FOCUS_CARD_COMPONENT_BOOL, isSelected);

            onSelected?.Invoke(model.id, isSelected);
            RefreshEquipButtonVisibility();
            RefreshAssignedSlotTextVisibility();
        }

        internal void RefreshFavoriteMarkVisibility()
        {
            if (favActivatedImage != null)
            {
                if (model.isFavorite)
                    favActivatedImage.Show();
                else
                    favActivatedImage.Hide();
            }

            if (favDeactivatedImage != null)
            {
                if (model.isFavorite)
                    favDeactivatedImage.Hide();
                else
                    favDeactivatedImage.Show();
            }
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

            assignedInCurrentSlotMarkImage.gameObject.SetActive(model.isAssignedInSelectedSlot);
            RefreshAssignedSlotTextVisibility();
            RefreshEquipButtonVisibility();
        }

        internal void RefreshEquipButtonVisibility()
        {
            if (equipButton == null)
                return;

            if (model.isSelected && !model.isAssignedInSelectedSlot)
                equipButton.Show();
            else
                equipButton.Hide();
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