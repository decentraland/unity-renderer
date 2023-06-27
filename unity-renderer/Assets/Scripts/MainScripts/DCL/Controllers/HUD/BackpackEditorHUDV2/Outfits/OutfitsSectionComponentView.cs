using Cysharp.Threading.Tasks;
using DCL;
using DG.Tweening;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public event Action<OutfitItem> OnOutfitDiscarded;
        public event Action<OutfitItem> OnOutfitSaved;
        public event Action<OutfitItem[]> OnUpdateLocalOutfits;
        public event Action OnTrySaveAsGuest;

        private readonly AvatarModel currentAvatarModel = new AvatarModel();
        private OutfitItem[] outfits;
        private int indexToBeDiscarded;

        public override void Awake()
        {
            base.Awake();

            outfits = new OutfitItem[]
            {
                new () { slot = 0 },
                new () { slot = 1 },
                new () { slot = 2 },
                new () { slot = 3 },
                new () { slot = 4 }
            };

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
            OnOutfitDiscarded?.Invoke(outfits[indexToBeDiscarded]);
            DeleteAnimation(outfitComponentViews[indexToBeDiscarded].transform);
            outfits[indexToBeDiscarded] = new OutfitItem() { slot = indexToBeDiscarded };
            OnUpdateLocalOutfits?.Invoke(outfits);
            discardOutfitModal.SetActive(false);
        }

        public override void RefreshControl() { }

        private void DiscardOutfit(int outfitIndex)
        {
            if (outfits[outfitIndex].outfit == null) return;

            indexToBeDiscarded = outfitIndex;
            discardOutfitModal.SetActive(true);
        }

        public void UpdateAvatarPreview(AvatarModel newAvatarModel)
        {
            currentAvatarModel.CopyFrom(newAvatarModel);
            characterPreviewController.SetEnabled(true);
            characterPreviewController.TryUpdateModelAsync(currentAvatarModel);
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

        public async UniTaskVoid ShowOutfits(OutfitItem[] outfitsToShow)
        {
            SetSlotsAsLoading(outfitsToShow);
            AudioScriptableObjects.listItemAppear.ResetPitch();

            foreach (OutfitItem outfitItem in outfitsToShow)
            {
                outfits[outfitItem.slot] = outfitItem;

                if (string.IsNullOrEmpty(outfitItem.outfit.bodyShape))
                    continue;

                outfitComponentViews[outfitItem.slot].SetIsEmpty(false);
                outfitComponentViews[outfitItem.slot].SetOutfit(outfitItem);
                await characterPreviewController.TryUpdateModelAsync(GenerateAvatarModel(outfitItem));
                Texture2D bodySnapshot = await characterPreviewController.TakeBodySnapshotAsync();
                AudioScriptableObjects.listItemAppear.Play(true);
                outfitComponentViews[outfitItem.slot].SetOutfitPreviewImage(bodySnapshot);
                outfitComponentViews[outfitItem.slot].SetIsLoading(false);
            }

            await characterPreviewController.TryUpdateModelAsync(currentAvatarModel);
        }

        private void SetSlotsAsLoading(OutfitItem[] outfitsToShow)
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

        private AvatarModel GenerateAvatarModel(OutfitItem outfitItem)
        {
            AvatarModel avatarModel = new AvatarModel();
            avatarModel.CopyFrom(currentAvatarModel);
            avatarModel.bodyShape = outfitItem.outfit.bodyShape;
            avatarModel.wearables = new List<string>(outfitItem.outfit.wearables.ToList());
            avatarModel.eyeColor = outfitItem.outfit.eyes.color;
            avatarModel.hairColor = outfitItem.outfit.hair.color;
            avatarModel.skinColor = outfitItem.outfit.skin.color;
            avatarModel.forceRender = new HashSet<string>(outfitItem.outfit.forceRender);
            return avatarModel;
        }

        private void OnEquipOutfit(OutfitItem outfitItem) =>
            OnOutfitEquipped?.Invoke(outfitItem);

        private void OnSaveOutfit(int outfitIndex) =>
            SaveOutfitAsync(outfitIndex).Forget();

        public void SetIsGuest(bool isGuest)
        {
            this.isGuest = isGuest;
        }

        private async UniTaskVoid SaveOutfitAsync(int outfitIndex)
        {
            if (isGuest)
            {
                OnTrySaveAsGuest?.Invoke();
                return;
            }

            outfitComponentViews[outfitIndex].SetIsLoading(true);
            outfitComponentViews[outfitIndex].SetIsEmpty(false);

            var outfitItem = new OutfitItem()
            {
                outfit = new OutfitItem.Outfit()
                {
                    bodyShape = currentAvatarModel.bodyShape,
                    eyes = new OutfitItem.eyes() { color = currentAvatarModel.eyeColor },
                    hair = new OutfitItem.hair() { color = currentAvatarModel.hairColor },
                    skin = new OutfitItem.skin() { color = currentAvatarModel.skinColor },
                    wearables = new List<string>(currentAvatarModel.wearables).ToArray(),
                    forceRender = new List<string>(currentAvatarModel.forceRender).ToArray()
                },
                slot = outfitIndex
            };

            outfitComponentViews[outfitIndex].SetModel(new OutfitComponentModel() { outfitItem = outfitItem });
            outfits[outfitIndex] = outfitItem;
            OnOutfitSaved?.Invoke(outfitItem);
            Texture2D bodySnapshot = await characterPreviewController.TakeBodySnapshotAsync();
            outfitComponentViews[outfitIndex].SetOutfitPreviewImage(bodySnapshot);
            outfitComponentViews[outfitIndex].SetIsLoading(false);
            SaveAnimation(outfitComponentViews[outfitIndex].gameObject.transform);
            OnUpdateLocalOutfits?.Invoke(outfits);
        }

        private void SaveAnimation(Transform transformToAnimate) =>
            transformToAnimate.DOJump(transformToAnimate.position, 20, 1, 0.6f);

        private void DeleteAnimation(Transform transformToAnimate) =>
            transformToAnimate.DOPunchPosition(new Vector3(5, 2, 1), 0.6f);
    }
}
