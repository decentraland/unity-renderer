using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
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
    private AvatarModel currentAvatarModel;

    public override void Awake()
    {
        base.Awake();
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackButtonPressed?.Invoke());
        foreach (OutfitComponentView outfitComponentView in outfitComponentViews)
        {
            outfitComponentView.OnEquipOutfit += OnEquipOutfit;
            outfitComponentView.OnSaveOutfit += OnSaveOutfit;
        }
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

    public void ShowOutfits(OutfitItem[] outfits)
    {
        for (int i = 0; i < outfitComponentViews.Length; i++)
        {
            if (i < outfits.Length)
            {
                outfitComponentViews[i].SetIsEmpty(false);
                outfitComponentViews[i].SetOutfit(outfits[i]);
            }
            else
            {
                outfitComponentViews[i].SetIsEmpty(true);
            }
        }
    }

    private void OnEquipOutfit(OutfitItem outfitItem)
    {
        OnOutfitEquipped?.Invoke(outfitItem);
    }

    private int lastIndex;

    private void OnSaveOutfit(int outfitIndex)
    {
        lastIndex = outfitIndex;
        Debug.Log("Save outfit image");
        characterPreviewController.TakeSnapshots(OnSnapshotSuccess, OnSnapshotFailed);
        outfitComponentViews[outfitIndex].SetIsEmpty(false);
        outfitComponentViews[outfitIndex].SetModel(new OutfitComponentModel(){outfitItem = new OutfitItem()
        {
            outfit = new OutfitItem.Outfit()
            {
                bodyShape = currentAvatarModel.bodyShape,
                eyes = new OutfitItem.ElementColor(){color = currentAvatarModel.eyeColor},
                hair = new OutfitItem.ElementColor(){color = currentAvatarModel.hairColor},
                skin = new OutfitItem.ElementColor(){color = currentAvatarModel.skinColor},
                wearables = currentAvatarModel.wearables.ToArray()
            },
            slot = outfitIndex
        }});
    }

    private void OnSnapshotFailed()
    {
        Debug.Log("Snapshot failed");
    }

    private void OnSnapshotSuccess(Texture2D face256, Texture2D body)
    {
        Debug.Log("success");
        outfitComponentViews[lastIndex].SetOutfitPreviewImage(body);
        outfitComponentViews[lastIndex].SetIsEmpty(false);
    }

    public override void RefreshControl()
    {
    }
}
