using Cysharp.Threading.Tasks;
using System;

namespace DCL.Backpack
{
    public interface IOutfitsSectionComponentView
    {
        event Action<OutfitItem> OnOutfitEquipped;
        event Action<OutfitItem> OnOutfitDiscarded;
        event Action<OutfitItem> OnOutfitSaved;
        event Action<OutfitItem[]> OnUpdateLocalOutfits;
        event Action OnTrySaveAsGuest;

        UniTaskVoid ShowOutfits(OutfitItem[] outfitsToShow);
        void UpdateAvatarPreview(AvatarModel newAvatarModel);
        void SetIsGuest(bool isGuest);
    }
}
