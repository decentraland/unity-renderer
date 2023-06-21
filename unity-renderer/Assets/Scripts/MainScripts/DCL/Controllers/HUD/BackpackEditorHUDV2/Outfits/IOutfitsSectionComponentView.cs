using Cysharp.Threading.Tasks;
using System;

public interface IOutfitsSectionComponentView
{
    event Action<OutfitItem> OnOutfitEquipped;
    event Action<OutfitItem> OnOutfitDiscarded;
    event Action<OutfitItem> OnOutfitSaved;
    event Action<OutfitItem[]> OnUpdateLocalOutfits;

    UniTaskVoid ShowOutfits(OutfitItem[] outfitsToShow);
    void UpdateAvatarPreview(AvatarModel newAvatarModel);
}
