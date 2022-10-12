using System;
using UnityEngine;

public class AvatarEditorHUDAnimationController : IDisposable
{

    private CharacterPreviewController characterPreviewController;
    private AvatarEditorHUDView hudView;
    internal string activeCategory;
    private int currentAnimationIndexShown;

    public AvatarEditorHUDAnimationController(AvatarEditorHUDView avatarEditorHUDView)
    {
        this.hudView = avatarEditorHUDView;
        
        characterPreviewController = hudView.characterPreviewController;
        hudView.OnAvatarAppearFeedback += AvatarAppearFeedback;
        hudView.OnRandomize += OnClickRandomize;
        for (int i = 0; i < hudView.wearableGridPairs.Length; i++)
        {
            hudView.wearableGridPairs[i].selector.OnItemClicked += OnSelectWearable;
        }
        hudView.collectiblesItemSelector.OnItemClicked += OnSelectWearable;
    }
    
    public void AvatarAppearFeedback(AvatarModel avatarModelToUpdate)
    {
        if (!string.IsNullOrEmpty(activeCategory))
        {
            PlayAnimation(avatarModelToUpdate);
        }
    }

    private void OnClickRandomize()
    {
        activeCategory = "";
    }
    
    private void PlayAnimation(AvatarModel avatarModelToUpdate)
    {
        avatarModelToUpdate.expressionTriggerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        characterPreviewController.PlayEmote(activeCategory, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    public void OnSelectWearable(string wearableId)
    {
        CatalogController.wearableCatalog.TryGetValue(wearableId, out var wearable);
        switch (wearable.data.category)
        {
            case WearableLiterals.Categories.FEET:
                activeCategory = GetRandomizedName("Outfit_Shoes_v0",2);
                break;
            case WearableLiterals.Categories.LOWER_BODY:
                activeCategory = GetRandomizedName("Outfit_Lower_v0",3);
                break;
            case WearableLiterals.Categories.UPPER_BODY:
                activeCategory = GetRandomizedName("Outfit_Upper_v0",3);
                break;
            case "eyewear":
                activeCategory = GetRandomizedName("Outfit_Accessories_v0",3);
                break;
            case "tiara":
                activeCategory = GetRandomizedName("Outfit_Accessories_v0",3);
                break;
            case "earring":
                activeCategory = GetRandomizedName("Outfit_Accessories_v0",3);
                break;
            case "hat":
                activeCategory = GetRandomizedName("Outfit_Accessories_v0",3);
                break;
            case "top_head":
                activeCategory = GetRandomizedName("Outfit_Accessories_v0",3);
                break;
            case "helmet":
                activeCategory = GetRandomizedName("Outfit_Accessories_v0",3);
                break;
            case "mask":
                activeCategory = GetRandomizedName("Outfit_Accessories_v0",3);
                break;
            case "skin":
                activeCategory = GetRandomizedName("Outfit_Upper_v0",3);
                break;
            default:
                activeCategory = "";
                break;
        }
    }

    private string GetRandomizedName(string baseString, int limit)
    {
        currentAnimationIndexShown = (currentAnimationIndexShown + 1) % limit;
        return baseString + (currentAnimationIndexShown + 1);
    }

    public void Dispose()
    {
        hudView.OnAvatarAppearFeedback -= AvatarAppearFeedback;
        hudView.OnRandomize -= OnClickRandomize;
        for (int i = 0; i < hudView.wearableGridPairs.Length; i++)
        {
            hudView.wearableGridPairs[i].selector.OnItemClicked -= OnSelectWearable;
        }
        hudView.collectiblesItemSelector.OnItemClicked -= OnSelectWearable;
    }
}
