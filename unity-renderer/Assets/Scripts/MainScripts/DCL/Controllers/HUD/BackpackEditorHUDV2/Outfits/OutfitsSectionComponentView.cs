using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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
    private IUserProfileBridge userProfileBridge;
    private DataStore dataStore;

    public event Action OnBackButtonPressed;
    public event Action<OutfitItem> OnOutfitEquipped;
    public event Action<OutfitItem[]> OnUpdateLocalOutfits;
    private readonly AvatarModel currentAvatarModel = new AvatarModel();
    private OutfitItem[] outfits;
    private int indexToBeDiscarded;

    public override void Awake()
    {
        base.Awake();
        outfits = new OutfitItem[]
        {
            new (){slot = 0},
            new (){slot = 1},
            new (){slot = 2},
            new (){slot = 3},
            new (){slot = 4}
        };

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackButtonPressed?.Invoke());
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
        outfits[indexToBeDiscarded] = new OutfitItem(){slot = indexToBeDiscarded};
        OnUpdateLocalOutfits?.Invoke(outfits);
        discardOutfitModal.SetActive(false);
    }

    public override void RefreshControl()
    {
    }

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

    public void Initialize(ICharacterPreviewFactory characterPreviewFactory, IUserProfileBridge profileBridge, DataStore dtaStore)
    {
        characterPreviewController = characterPreviewFactory.Create(
            loadingMode: CharacterPreviewMode.WithoutHologram,
            renderTexture: (RenderTexture) avatarPreviewImage.texture,
            isVisible: false,
            previewCameraFocus: PreviewCameraFocus.DefaultEditing,
            isAvatarShadowActive: true);
        characterPreviewController.SetFocus(PreviewCameraFocus.DefaultEditing);
        this.userProfileBridge = profileBridge;
        this.dataStore = dtaStore;
    }

    public async UniTaskVoid ShowOutfits(OutfitItem[] outfitsToShow)
    {
        SetSlotsAsLoading(outfitsToShow);
        foreach (OutfitItem outfitItem in outfitsToShow)
        {
            outfits[outfitItem.slot] = outfitItem;

            if (string.IsNullOrEmpty(outfitItem.outfit.bodyShape))
                continue;

            outfitComponentViews[outfitItem.slot].SetIsEmpty(false);
            outfitComponentViews[outfitItem.slot].SetOutfit(outfitItem);
            await characterPreviewController.TryUpdateModelAsync(GenerateAvatarModel(outfitItem));
            Texture2D bodySnapshot = await characterPreviewController.TakeBodySnapshotAsync();
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
            else
            {
                outfitComponentViews[outfitItem.slot].SetIsLoading(true);
            }
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

    private async UniTaskVoid SaveOutfitAsync(int outfitIndex)
    {
        if (userProfileBridge.GetOwn().isGuest)
        {
            dataStore.HUDs.connectWalletModalVisible.Set(true);
            return;
        }

        outfitComponentViews[outfitIndex].SetIsLoading(true);
        outfitComponentViews[outfitIndex].SetIsEmpty(false);
        var outfitItem = new OutfitItem()
        {
            outfit = new OutfitItem.Outfit()
            {
                bodyShape = currentAvatarModel.bodyShape,
                eyes = new OutfitItem.eyes(){ color = currentAvatarModel.eyeColor},
                hair = new OutfitItem.hair(){ color = currentAvatarModel.hairColor},
                skin = new OutfitItem.skin(){ color = currentAvatarModel.skinColor},
                wearables = new List<string>(currentAvatarModel.wearables).ToArray(),
                forceRender = new List<string>(currentAvatarModel.forceRender).ToArray()
            },
            slot = outfitIndex
        };

        outfitComponentViews[outfitIndex].SetModel(new OutfitComponentModel(){outfitItem = outfitItem});
        outfits[outfitIndex] = outfitItem;
        Texture2D bodySnapshot = await characterPreviewController.TakeBodySnapshotAsync();
        outfitComponentViews[outfitIndex].SetOutfitPreviewImage(bodySnapshot);
        outfitComponentViews[outfitIndex].SetIsLoading(false);
        OnUpdateLocalOutfits?.Invoke(outfits);
    }
}
