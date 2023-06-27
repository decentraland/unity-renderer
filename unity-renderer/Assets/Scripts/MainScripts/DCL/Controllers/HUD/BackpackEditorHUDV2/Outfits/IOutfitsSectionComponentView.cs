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

        public void SetSlotsAsLoading(OutfitItem[] outfitsToShow);
        UniTask<bool> ShowOutfit(OutfitItem outfit, AvatarModel newModel);
        void UpdateAvatarPreview(AvatarModel newAvatarModel);
        void SetIsGuest(bool isGuest);
    }
}
