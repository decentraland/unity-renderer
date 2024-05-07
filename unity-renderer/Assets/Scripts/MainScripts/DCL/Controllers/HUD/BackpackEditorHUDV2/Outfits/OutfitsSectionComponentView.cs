using Cysharp.Threading.Tasks;
using DG.Tweening;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class OutfitsSectionComponentView : BaseComponentView, IOutfitsSectionComponentView
    {
        [SerializeField] internal Button backButton;
        [SerializeField] internal OutfitComponentView[] outfitComponentViews;
        [SerializeField] private RawImage avatarPreviewImage;
        [SerializeField] private GameObject discardOutfitModal;
        [SerializeField] private Button confirmDiscardOutfit;
        [SerializeField] private Button cancelDiscardOutfit;
        [SerializeField] private Button closeDiscardOutfit;

        private ICharacterPreviewController characterPreviewController;
        private bool isGuest;

        public event Action OnBackButtonPressed;
        public event Action<OutfitItem> OnOutfitEquipped;
        public event Action<int> OnOutfitDiscarded;
        public event Action<int> OnOutfitLocalSave;
        public event Action OnTrySaveAsGuest;

        private int indexToBeDiscarded;

        public override void Awake()
        {
            base.Awake();

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => OnBackButtonPressed?.Invoke());

            foreach (OutfitComponentView outfitComponentView in outfitComponentViews)
            {
                outfitComponentView.OnEquipOutfit += OnEquipOutfit;
                outfitComponentView.OnSaveOutfit += OnSaveOutfit;
                outfitComponentView.OnDiscardOutfit += DiscardOutfit;
            }

            if (confirmDiscardOutfit != null)
            {
                confirmDiscardOutfit.onClick.RemoveAllListeners();
                confirmDiscardOutfit.onClick.AddListener(CompleteDiscardOutfit);
            }

            if (cancelDiscardOutfit != null)
            {
                cancelDiscardOutfit.onClick.RemoveAllListeners();
                cancelDiscardOutfit.onClick.AddListener(() => discardOutfitModal.SetActive(false));
            }

            if (closeDiscardOutfit != null)
            {
                closeDiscardOutfit.onClick.RemoveAllListeners();
                closeDiscardOutfit.onClick.AddListener(() => discardOutfitModal.SetActive(false));
            }
        }

        private void CompleteDiscardOutfit()
        {
            outfitComponentViews[indexToBeDiscarded].SetIsEmpty(true);
            OnOutfitDiscarded?.Invoke(indexToBeDiscarded);
            DeleteAnimation(outfitComponentViews[indexToBeDiscarded].transform);
            discardOutfitModal.SetActive(false);
        }

        public override void RefreshControl() { }

        private void DiscardOutfit(int outfitIndex)
        {
            indexToBeDiscarded = outfitIndex;
            discardOutfitModal.SetActive(true);
        }

        public void Initialize(ICharacterPreviewFactory characterPreviewFactory)
        {
            characterPreviewController = characterPreviewFactory.Create(
                loadingMode: CharacterPreviewMode.WithoutHologram,
                renderTexture: (RenderTexture)avatarPreviewImage.texture,
                isVisible: false,
                previewCameraFocus: PreviewCameraFocus.DefaultEditing,
                isAvatarShadowActive: true);

            characterPreviewController.SetFocus(PreviewCameraFocus.BodySnapshot);
        }

        public async UniTask<bool> ShowOutfit(OutfitItem outfit, AvatarModel newModel, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(outfit.outfit.bodyShape))
                return false;

            characterPreviewController.SetEnabled(true);
            outfitComponentViews[outfit.slot].SetIsEmpty(false);
            outfitComponentViews[outfit.slot].SetOutfit(outfit);
            await characterPreviewController.TryUpdateModelAsync(newModel, ct);
            Texture2D bodySnapshot = await characterPreviewController.TakeBodySnapshotAsync();
            AudioScriptableObjects.listItemAppear.Play(true);
            outfitComponentViews[outfit.slot].SetOutfitPreviewImage(bodySnapshot);
            outfitComponentViews[outfit.slot].SetIsLoading(false);
            characterPreviewController.SetEnabled(false);
            return true;
        }

        public void SetSlotsAsLoading(OutfitItem[] outfitsToShow)
        {
            foreach (var outfitItem in outfitsToShow)
            {
                if (string.IsNullOrEmpty(outfitItem.outfit.bodyShape))
                {
                    outfitComponentViews[outfitItem.slot].SetIsLoading(false);
                    outfitComponentViews[outfitItem.slot].SetIsEmpty(true);
                }
                else { outfitComponentViews[outfitItem.slot].SetIsLoading(true); }
            }
        }

        private void OnEquipOutfit(OutfitItem outfitItem) =>
            OnOutfitEquipped?.Invoke(outfitItem);

        private void OnSaveOutfit(int outfitIndex)
        {
            if (isGuest)
            {
                OnTrySaveAsGuest?.Invoke();
                return;
            }

            outfitComponentViews[outfitIndex].SetIsLoading(true);
            outfitComponentViews[outfitIndex].SetIsEmpty(false);
            OnOutfitLocalSave?.Invoke(outfitIndex);
            SaveAnimation(outfitComponentViews[outfitIndex].transform);
        }

        public void SetIsGuest(bool guest)
        {
            this.isGuest = guest;
        }

        private void SaveAnimation(Transform transformToAnimate) =>
            transformToAnimate.DOJump(transformToAnimate.position, 20, 1, 0.6f);

        private void DeleteAnimation(Transform transformToAnimate) =>
            transformToAnimate.DOPunchPosition(new Vector3(5, 2, 1), 0.6f);
    }
}
