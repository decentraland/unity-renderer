using System;

public class AvatarEditorHUDAnimationController : IAvatarEditorHUDAnimationController
{

    private CharacterPreviewController characterPreviewController;
    private AvatarEditorHUDView hudView;
    private string activeCategory;


    public void Initialize(AvatarEditorHUDView avatarEditorHUDView)
    {
        this.characterPreviewController = hudView.characterPreviewController;
        this.hudView = avatarEditorHUDView;
        
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
            case WearableLiterals.Categories.BODY_SHAPE:
                activeCategory = "";
                break;
            default:
                activeCategory = GetRandomizedName("Outfit_Upper_v0",3);
                break;
        }
    }

    private string GetRandomizedName(string baseString, int limit)
    {
        return baseString + UnityEngine.Random.Range(1, limit+1);
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
