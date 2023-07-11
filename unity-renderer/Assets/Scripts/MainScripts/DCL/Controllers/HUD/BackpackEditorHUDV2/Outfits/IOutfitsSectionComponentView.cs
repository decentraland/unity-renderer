using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL.Backpack
{
    public interface IOutfitsSectionComponentView
    {
        event Action<OutfitItem> OnOutfitEquipped;
        event Action<int> OnOutfitDiscarded;
        event Action<int> OnOutfitLocalSave;
        event Action OnTrySaveAsGuest;

        public void SetSlotsAsLoading(OutfitItem[] outfitsToShow);
        UniTask<bool> ShowOutfit(OutfitItem outfit, AvatarModel newModel, CancellationToken ct);
        void SetIsGuest(bool guest);
    }
}
