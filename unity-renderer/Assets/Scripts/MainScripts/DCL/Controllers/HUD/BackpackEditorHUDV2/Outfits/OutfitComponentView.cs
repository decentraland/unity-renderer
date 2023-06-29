using System;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class OutfitComponentView : BaseComponentView<OutfitComponentModel>, IOutfitComponentView
    {
        [SerializeField] internal GameObject emptyState;
        [SerializeField] internal GameObject filledState;
        [SerializeField] internal GameObject loadingState;
        [SerializeField] internal GameObject[] hoverStates;
        [SerializeField] internal GameObject[] normalStates;
        [SerializeField] internal Button equipButton;
        [SerializeField] internal Button saveOutfitButton;
        [SerializeField] internal Button discardOutfitButton;
        [SerializeField] internal RawImage outfitPreviewImage;
        [SerializeField] private int outfitIndex;

        public event Action<OutfitItem> OnEquipOutfit;
        public event Action<int> OnSaveOutfit;
        public event Action<int> OnDiscardOutfit;

        public override void Awake()
        {
            base.Awake();
            InitializeButtonEvents();
        }

        private void InitializeButtonEvents()
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(() => OnEquipOutfit?.Invoke(model.outfitItem));
            saveOutfitButton.onClick.RemoveAllListeners();
            saveOutfitButton.onClick.AddListener(() => OnSaveOutfit?.Invoke(outfitIndex));
            discardOutfitButton.onClick.RemoveAllListeners();
            discardOutfitButton.onClick.AddListener(() => OnDiscardOutfit?.Invoke(outfitIndex));
        }

        public override void RefreshControl() =>
            SetOutfit(model.outfitItem);

        public void SetOutfit(OutfitItem outfitItem) =>
            model.outfitItem = outfitItem;

        public void SetOutfitPreviewImage(Texture bodyTexture) =>
            outfitPreviewImage.texture = bodyTexture;

        public void SetIsEmpty(bool isEmpty)
        {
            emptyState.SetActive(isEmpty);
            filledState.SetActive(!isEmpty);
        }

        public void SetIsLoading(bool isLoading)
        {
            loadingState.SetActive(isLoading);

            if (!isLoading) return;
            emptyState.SetActive(false);
            filledState.SetActive(false);
        }

        public override void OnFocus()
        {
            base.OnFocus();

            foreach (GameObject hoverState in hoverStates)
                hoverState.SetActive(true);

            foreach (GameObject normalState in normalStates)
                normalState.SetActive(false);
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();

            foreach (GameObject normalState in normalStates)
                normalState.SetActive(true);

            foreach (GameObject hoverState in hoverStates)
                hoverState.SetActive(false);
        }
    }
}
