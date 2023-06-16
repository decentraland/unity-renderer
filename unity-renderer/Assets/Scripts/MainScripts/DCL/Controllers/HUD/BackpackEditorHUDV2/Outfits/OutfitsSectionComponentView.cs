using Cysharp.Threading.Tasks;
using DCL.Interface;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OutfitsSectionComponentView : BaseComponentView
{

    [SerializeField] internal Button backButton;
    [SerializeField] internal OutfitComponentView[] outfitComponentViews;
    [SerializeField] private RawImage avatarPreviewImage;

    private ICharacterPreviewController characterPreviewController;

    public event Action OnBackButtonPressed;
    public event Action<OutfitItem> OnOutfitEquipped;
    public event Action<OutfitItem[]> OnSaveOutfits;
    private AvatarModel currentAvatarModel;
    private OutfitItem[] outfits;

    public override void Awake()
    {
        base.Awake();
        outfits = new OutfitItem[]
        {
            new OutfitItem(){slot = 0},
            new OutfitItem(){slot = 1},
            new OutfitItem(){slot = 2},
            new OutfitItem(){slot = 3},
            new OutfitItem(){slot = 4}
        };

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackButtonPressed?.Invoke());
        foreach (OutfitComponentView outfitComponentView in outfitComponentViews)
        {
            outfitComponentView.OnEquipOutfit += OnEquipOutfit;
            outfitComponentView.OnSaveOutfit += OnSaveOutfit;
            outfitComponentView.OnDiscardOutfit += DiscardOutfit;
        }
    }

    private void DiscardOutfit(int outfitIndex)
    {
        outfitComponentViews[outfitIndex].SetIsEmpty(true);
        outfits[outfitIndex] = null;
        OnSaveOutfits?.Invoke(outfits);
    }

    public void UpdateAvatarPreview(AvatarModel newAvatarModel)
    {
        currentAvatarModel = newAvatarModel;
        characterPreviewController.SetEnabled(true);
        characterPreviewController.TryUpdateModelAsync(currentAvatarModel);
    }

    public void Initialize(ICharacterPreviewFactory characterPreviewFactory)
    {
        characterPreviewController = characterPreviewFactory.Create(
            loadingMode: CharacterPreviewMode.WithoutHologram,
            renderTexture: (RenderTexture) avatarPreviewImage.texture,
            isVisible: false,
            previewCameraFocus: PreviewCameraFocus.DefaultEditing,
            isAvatarShadowActive: true);
        characterPreviewController.SetFocus(PreviewCameraFocus.DefaultEditing);
    }

    public async UniTaskVoid ShowOutfits(OutfitItem[] outfitsToShow)
    {
        for (int i = 0; i < outfitComponentViews.Length; i++)
        {
            if (i < outfitsToShow.Length && !string.IsNullOrEmpty(outfitsToShow[i].outfit.bodyShape))
            {
                outfitComponentViews[i].SetIsEmpty(false);
                outfitComponentViews[i].SetOutfit(outfitsToShow[i]);
                AvatarModel avatarModel = currentAvatarModel;
                avatarModel.bodyShape = outfitsToShow[i].outfit.bodyShape;
                avatarModel.wearables = outfitsToShow[i].outfit.wearables.ToList();
                avatarModel.eyeColor = outfitsToShow[i].outfit.eyes.color;
                avatarModel.hairColor = outfitsToShow[i].outfit.hair.color;
                avatarModel.skinColor = outfitsToShow[i].outfit.skin.color;
                await characterPreviewController.TryUpdateModelAsync(avatarModel);
                Texture2D bodySnapshot = await characterPreviewController.TakeBodySnapshotAsync();
                outfitComponentViews[i].SetOutfitPreviewImage(bodySnapshot);
            }
            else
            {
                outfitComponentViews[i].SetIsEmpty(true);
            }
        }
    }

    private void OnEquipOutfit(OutfitItem outfitItem) =>
        OnOutfitEquipped?.Invoke(outfitItem);

    private int lastIndex;

    private void OnSaveOutfit(int outfitIndex)
    {
        lastIndex = outfitIndex;
        characterPreviewController.TakeSnapshots(OnSnapshotSuccess, OnSnapshotFailed);
        outfitComponentViews[outfitIndex].SetIsEmpty(false);

        var outfitItem = new OutfitItem()
        {
            outfit = new OutfitItem.Outfit()
            {
                bodyShape = currentAvatarModel.bodyShape,
                eyes = new OutfitItem.eyes(){ color = currentAvatarModel.eyeColor},
                hair = new OutfitItem.hair(){ color = currentAvatarModel.hairColor},
                skin = new OutfitItem.skin(){ color = currentAvatarModel.skinColor},
                wearables = currentAvatarModel.wearables.ToArray()
            },
            slot = outfitIndex
        };

        outfitComponentViews[outfitIndex].SetModel(new OutfitComponentModel(){outfitItem = outfitItem});
        outfits[outfitIndex] = outfitItem;
        OnSaveOutfits?.Invoke(outfits);
    }

    private void OnSnapshotFailed()
    {
        Debug.Log("Snapshot failed");
    }

    private void OnSnapshotSuccess(Texture2D face256, Texture2D body)
    {
        outfitComponentViews[lastIndex].SetOutfitPreviewImage(body);
        outfitComponentViews[lastIndex].SetIsEmpty(false);
    }

    public override void RefreshControl()
    {
    }
}
