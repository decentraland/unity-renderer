using Cysharp.Threading.Tasks;
using DCL;
using DCL.Backpack;
using DCL.Interface;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Backpack
{
    public class OutfitsController : IDisposable
    {
        private readonly LambdaOutfitsService lambdaOutfitsService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IOutfitsSectionComponentView view;
        private readonly IBackpackAnalyticsService backpackAnalyticsService;
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

            view.OnOutfitEquipped += OutfitEquip;
            view.OnOutfitDiscarded += DiscardOutfit;
            view.OnOutfitSaved += SaveOutfit;
            view.OnUpdateLocalOutfits += UpdateLocalOutfits;

            dataStore.HUDs.avatarEditorVisible.OnChange += ChangedVisibility;
        }

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
            cts?.Cancel();
            cts?.Dispose();
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
            view.OnUpdateLocalOutfits -= UpdateLocalOutfits;
            view.OnOutfitEquipped -= OutfitEquip;
        }
    }
}
