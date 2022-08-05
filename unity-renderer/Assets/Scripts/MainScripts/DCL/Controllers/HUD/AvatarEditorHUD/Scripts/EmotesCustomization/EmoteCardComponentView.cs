using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.EmotesCustomization
{
    public class EmoteCardComponentView : BaseComponentView, IEmoteCardComponentView, IComponentModelConfig<EmoteCardComponentModel>
    {
        internal static readonly int ON_SELECTED_CARD_COMPONENT_BOOL = Animator.StringToHash("OnSelected");

        [Header("Prefab References")]
        [SerializeField] internal ImageComponentView emoteImage;
        [SerializeField] internal GameObject emoteNameContainer;
        [SerializeField] internal TMP_Text emoteNameText;
        [SerializeField] internal TMP_Text assignedSlotNumberText;
        [SerializeField] internal ButtonComponentView mainButton;
        [SerializeField] internal ButtonComponentView infoButton;
        [SerializeField] internal GameObject cardSelectionFrame;
        [SerializeField] internal Animator selectionAnimator;
        [SerializeField] internal Image rarityMark;
        [SerializeField] internal Transform emoteInfoAnchor;
        [SerializeField] internal GameObject loadingSpinnerGO;

        [Header("Configuration")]
        [SerializeField] internal Sprite defaultEmotePicture;
        [SerializeField] internal Sprite nonEmoteAssignedPicture;
        [SerializeField] internal List<EmoteRarity> rarityColors;
        [SerializeField] internal EmoteCardComponentModel model;

        public Button.ButtonClickedEvent onMainClick => mainButton?.onClick;
        public Button.ButtonClickedEvent onInfoClick => infoButton?.onClick;
        
        public event Action<string> onEmoteSelected;

        public override void Awake()
        {
            base.Awake();

            if (emoteImage != null)
                emoteImage.OnLoaded += OnEmoteImageLoaded;

            if (cardSelectionFrame != null)
                cardSelectionFrame.SetActive(false);
        }

        public void Configure(EmoteCardComponentModel newModel)
        {
            if (model == newModel)
                return;

            model = newModel;
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
            SetIsCollectible(model.isCollectible);
            SetAsLoading(model.isLoading);
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

        public void UnassignSlot() { AssignSlot(-1); }

        public void SetEmoteAsSelected(bool isSelected)
        {
            model.isSelected = isSelected;

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

        public void SetIsCollectible(bool isCollectible)
        {
            model.isCollectible = isCollectible;

            RefreshVisualCardStatus();
        }

        public void SetAsLoading(bool isLoading)
        {
            model.isLoading = isLoading;

            if (loadingSpinnerGO != null)
                loadingSpinnerGO.SetActive(isLoading);

            if (mainButton != null)
                mainButton.gameObject.SetActive(!isLoading);
        }

        internal void RefreshVisualCardStatus()
        {
            RefreshAssignedSlotTextVisibility();
            RefreshSelectionFrameVisibility();
            RefreshCardButtonsVisibility();
        }

        internal void RefreshAssignedSlotTextVisibility()
        {
            if (assignedSlotNumberText == null)
                return;

            assignedSlotNumberText.gameObject.SetActive(model.assignedSlot >= 0);
        }

        internal void RefreshSelectionFrameVisibility()
        {
            if (cardSelectionFrame == null)
                return;

            cardSelectionFrame.SetActive(model.isSelected || model.isAssignedInSelectedSlot);
        }

        internal void RefreshCardButtonsVisibility()
        {
            if (infoButton != null)
                infoButton.gameObject.SetActive(model.isCollectible);
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