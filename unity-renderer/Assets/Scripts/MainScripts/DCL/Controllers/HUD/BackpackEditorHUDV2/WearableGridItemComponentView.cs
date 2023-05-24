using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class WearableGridItemComponentView : BaseComponentView<WearableGridItemModel>
    {
        private const float DOUBLE_CLICK_MAX_DELAY = 0.75f;

        [SerializeField] internal NftRarityBackgroundSO rarityNftBackgrounds;
        [SerializeField] internal NFTTypeIconsAndColors nftTypesIcons;
        [SerializeField] internal Image nftBackground;
        [SerializeField] internal Image categoryImage;
        [SerializeField] internal Image categoryBackground;
        [SerializeField] internal GameObject selectedContainer;
        [SerializeField] internal GameObject equippedContainer;
        [SerializeField] internal GameObject isNewContainer;
        [SerializeField] internal ImageComponentView image;
        [SerializeField] internal Button interactButton;
        [SerializeField] internal Material grayScaleMaterial;
        [SerializeField] internal GameObject incompatibleContainer;
        [SerializeField] internal GameObject incompatibleTooltip;

        private string lastThumbnailUrl;

        public WearableGridItemModel Model => model;

        private int clicked = 0;
        private float clickTime = 0;

        public event Action<WearableGridItemModel> OnSelected;
        public event Action<WearableGridItemModel> OnEquipped;
        public event Action<WearableGridItemModel> OnUnequipped;

        private CancellationTokenSource cts;

        public override void Awake()
        {
            base.Awake();
            image.OnLoaded += PlayLoadingSound;
            cts = new CancellationTokenSource();

            interactButton.onClick.AddListener(() =>
            {
                clicked++;

                if (clicked == 1)
                {
                    clickTime = Time.time;
                    CancelDoubleClickCooldown();
                    DoubleClickCooldown(cts.Token).Forget();
                    OnSelected?.Invoke(model);
                }

                if (clicked > 1)
                {
                    CancelDoubleClickCooldown();
                    clicked = 0;
                    clickTime = 0;

                    if (model.IsEquipped)
                    {
                        if (!model.UnEquipAllowed)
                            return;

                        OnUnequipped?.Invoke(model);
                    }
                    else
                        OnEquipped?.Invoke(model);
                }
            });
        }

        private async UniTaskVoid DoubleClickCooldown(CancellationToken cancellationToken)
        {
            while (Time.time - clickTime < DOUBLE_CLICK_MAX_DELAY)
                await UniTask.NextFrame(cancellationToken);

            clicked = 0;
        }

        private void CancelDoubleClickCooldown()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        public override void Dispose()
        {
            image.OnLoaded -= PlayLoadingSound;
            CancelDoubleClickCooldown();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            RefreshControl();
            selectedContainer.SetActive(true);
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();
            selectedContainer.SetActive(false);
            RefreshControl();
            incompatibleTooltip.SetActive(false);
        }

        public override void RefreshControl()
        {
            selectedContainer.SetActive(model.IsSelected);
            equippedContainer.SetActive(model.IsEquipped);
            isNewContainer.SetActive(model.IsNew);

            // we gotta check for url changes, otherwise the image component will start a "loading" state, even if the url is the same
            if (lastThumbnailUrl != model.ImageUrl)
            {
                image.SetImage(model.ImageUrl);
                lastThumbnailUrl = model.ImageUrl;
            }

            string nftRarity = model.Rarity.ToString().ToLower();
            nftBackground.sprite = rarityNftBackgrounds.GetRarityImage(nftRarity);
            categoryBackground.color = nftTypesIcons.GetColor(nftRarity);
            categoryImage.sprite = nftTypesIcons.GetTypeImage(model.Category);

            if (model.IsCompatibleWithBodyShape)
                image.ImageComponent.material = null;
            else
                image.ImageComponent.material = grayScaleMaterial;

            incompatibleContainer.SetActive(!model.IsCompatibleWithBodyShape);
            interactButton.interactable = model.IsCompatibleWithBodyShape;
            incompatibleTooltip.SetActive(!model.IsCompatibleWithBodyShape && isFocused);
        }

        private void PlayLoadingSound(Sprite sprt)
        {
            AudioScriptableObjects.listItemAppear.Play(true);
        }

        public void Unselect()
        {
            model.IsSelected = false;
            RefreshControl();
        }

        public void Select()
        {
            model.IsSelected = true;
            RefreshControl();
        }
    }
}
