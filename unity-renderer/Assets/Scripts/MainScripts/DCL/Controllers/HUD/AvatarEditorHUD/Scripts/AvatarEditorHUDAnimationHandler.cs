using System;

public class AvatarEditorHUDAnimationHandler : IDisposable
{

    private readonly CharacterPreviewController characterPreviewController;
    private AvatarEditorHUDView hudView;
    private string activeCategory;
    static readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    

    public AvatarEditorHUDAnimationHandler(CharacterPreviewController characterPreviewController, AvatarEditorHUDView hudView)
    {
        this.characterPreviewController = characterPreviewController;
        this.hudView = hudView;
        hudView.OnAvatarAppear += PlayAvatarAnimation;
        hudView.OnRandomize += OnClickRandomize;
        for (int i = 0; i < hudView.wearableGridPairs.Length; i++)
        {
            hudView.wearableGridPairs[i].selector.OnItemClicked += OnSelectWearable;
        }
    }
    
    private void PlayAvatarAnimation(AvatarModel avatarModelToUpdate)
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
        var timestamp = (long) (DateTime.UtcNow - epochStart).TotalMilliseconds;
        avatarModelToUpdate.expressionTriggerTimestamp = timestamp;
        characterPreviewController.PlayEmote(activeCategory, timestamp);
    }

    private void OnSelectWearable(string wearableId)
    {
        CatalogController.wearableCatalog.TryGetValue(wearableId, out var wearable);

        
        switch (wearable.data.category)
        {
            case WearableLiterals.Categories.FEET:
                activeCategory = GetRandomizedName("Outfit_Shoes_v0",2);
                break;
            case WearableLiterals.Categories.LOWER_BODY:
                activeCategory = GetRandomizedName("Outfit_Lower_v0",2);
                break;
            case WearableLiterals.Categories.UPPER_BODY:
                activeCategory = GetRandomizedName("Outfit_Upper_v0",2);
                break;
            case "eyewear":
                activeCategory = "Outfit_Accessories";
                break;
            case "tiara":
                activeCategory = "Outfit_Accessories";
                break;
            case "earring":
                activeCategory = "Outfit_Accessories";
                break;
            case "hat":
                activeCategory = "Outfit_Accessories";
                break;
            case "top_head":
                activeCategory = "Outfit_Accessories";
                break;
            case "helmet":
                activeCategory = "Outfit_Accessories";
                break;
            case "mask":
                activeCategory = "Outfit_Accessories";
                break;
            default:
                activeCategory = "";
                break;
        }
    }

    private string GetRandomizedName(string baseString, int limit)
    {
        return baseString + UnityEngine.Random.Range(1, limit+1);
    }

    public void Dispose()
    {
        hudView.OnAvatarAppear -= PlayAvatarAnimation;
        hudView.OnRandomize -= OnClickRandomize;
        for (int i = 0; i < hudView.wearableGridPairs.Length; i++)
        {
            hudView.wearableGridPairs[i].selector.OnItemClicked -= OnSelectWearable;
        }
    }
}
