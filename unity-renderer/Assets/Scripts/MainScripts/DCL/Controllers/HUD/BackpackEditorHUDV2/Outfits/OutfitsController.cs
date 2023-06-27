using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DCL.Backpack
{
    public class OutfitsController : IDisposable
    {
        private readonly LambdaOutfitsService lambdaOutfitsService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IOutfitsSectionComponentView view;
        private readonly IBackpackAnalyticsService backpackAnalyticsService;
        private readonly DataStore dataStore;
        public event Action<OutfitItem> OnOutfitEquipped;

        private CancellationTokenSource cts;
        private OutfitItem[] localOutfits;
        private bool shouldDeploy;

        public OutfitsController(
            IOutfitsSectionComponentView view,
            LambdaOutfitsService lambdaOutfitsService,
            IUserProfileBridge userProfileBridge,
            DataStore dataStore,
            IBackpackAnalyticsService backpackAnalyticsService)
        {
            this.view = view;
            this.lambdaOutfitsService = lambdaOutfitsService;
            this.userProfileBridge = userProfileBridge;
            this.backpackAnalyticsService = backpackAnalyticsService;
            this.dataStore = dataStore;

            view.OnOutfitEquipped += OutfitEquip;
            view.OnOutfitDiscarded += DiscardOutfit;
            view.OnOutfitSaved += SaveOutfit;
            view.OnUpdateLocalOutfits += UpdateLocalOutfits;
            view.OnTrySaveAsGuest += ShowGuestModal;

            dataStore.HUDs.avatarEditorVisible.OnChange += ChangedVisibility;
            view.SetIsGuest(userProfileBridge.GetOwn().isGuest);
        }

        private void ShowGuestModal() =>
            dataStore.HUDs.connectWalletModalVisible.Set(true);

        private void SaveOutfit(OutfitItem outfit) =>
            backpackAnalyticsService.SendOutfitSave(outfit.slot);

        private void DiscardOutfit(OutfitItem outfit) =>
            backpackAnalyticsService.SendOutfitDelete(outfit.slot);

        private void OutfitEquip(OutfitItem outfit)
        {
            backpackAnalyticsService.SendOutfitEquipped(outfit.slot);
            OnOutfitEquipped?.Invoke(outfit);
        }

        private void ChangedVisibility(bool current, bool previous)
        {
            if (shouldDeploy)
                WebInterface.SaveUserOutfits(localOutfits);
        }

        private void UpdateLocalOutfits(OutfitItem[] outfits)
        {
            shouldDeploy = true;
            localOutfits = outfits;
        }

        public void RequestOwnedOutfits()
        {
            cts.SafeCancelAndDispose();
            cts = new CancellationTokenSource();
            RequestOwnedOutfitsAsync().Forget();
        }

        private async UniTask RequestOwnedOutfitsAsync()
        {
            (IReadOnlyList<OutfitItem> outfits, int totalAmount) requestOwnedOutfits = await lambdaOutfitsService.RequestOwnedOutfits(userProfileBridge.GetOwn().userId, cancellationToken: cts.Token);
            view.ShowOutfits(requestOwnedOutfits.outfits.ToArray()).Forget();
        }

        public void UpdateAvatarPreview(AvatarModel newAvatarModel) =>
            view.UpdateAvatarPreview(newAvatarModel);

        public void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            view.OnOutfitEquipped -= OutfitEquip;
            view.OnOutfitDiscarded -= DiscardOutfit;
            view.OnOutfitSaved -= SaveOutfit;
            view.OnUpdateLocalOutfits -= UpdateLocalOutfits;
            view.OnTrySaveAsGuest -= ShowGuestModal;

            dataStore.HUDs.avatarEditorVisible.OnChange -= ChangedVisibility;
        }
    }
}
