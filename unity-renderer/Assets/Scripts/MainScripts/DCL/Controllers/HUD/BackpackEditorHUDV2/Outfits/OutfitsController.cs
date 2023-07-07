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
        private CancellationTokenSource ctsShowOutfit;
        private OutfitItem[] localOutfits;
        private bool shouldDeploy;
        private AvatarModel currentAvatarModel;

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
            view.OnTrySaveAsGuest += ShowGuestModal;
            view.OnOutfitLocalSave += SaveOutfitLocally;

            localOutfits = new OutfitItem[]
            {
                new () { slot = 0 },
                new () { slot = 1 },
                new () { slot = 2 },
                new () { slot = 3 },
                new () { slot = 4 }
            };

            dataStore.HUDs.avatarEditorVisible.OnChange += ChangedVisibility;
        }

        private void SaveOutfitLocally(int outfitIndex)
        {
            ctsShowOutfit.SafeCancelAndDispose();
            ctsShowOutfit = new CancellationTokenSource();
            var outfitItem = new OutfitItem()
            {
                outfit = new OutfitItem.Outfit()
                {
                    bodyShape = currentAvatarModel.bodyShape,
                    eyes = new OutfitItem.eyes() { color = currentAvatarModel.eyeColor },
                    hair = new OutfitItem.hair() { color = currentAvatarModel.hairColor },
                    skin = new OutfitItem.skin() { color = currentAvatarModel.skinColor },
                    wearables = new List<string>(currentAvatarModel.wearables).ToArray(),
                    forceRender = new List<string>(currentAvatarModel.forceRender).ToArray()
                },
                slot = outfitIndex
            };

            localOutfits[outfitIndex] = outfitItem;
            view.ShowOutfit(outfitItem, GenerateAvatarModel(outfitItem), ctsShowOutfit.Token).Forget();
            shouldDeploy = true;

            backpackAnalyticsService.SendOutfitSave(outfitIndex);
        }

        private void ShowGuestModal() =>
            dataStore.HUDs.connectWalletModalVisible.Set(true);

        private void DiscardOutfit(int outfitIndex)
        {
            backpackAnalyticsService.SendOutfitDelete(outfitIndex);
            localOutfits[outfitIndex] = new () { slot = outfitIndex };
            shouldDeploy = true;
        }

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

        public void RequestOwnedOutfits()
        {
            cts.SafeCancelAndDispose();
            cts = new CancellationTokenSource();
            RequestOwnedOutfitsAsync(cts.Token).Forget();
        }

        private async UniTask RequestOwnedOutfitsAsync(CancellationToken ct)
        {
            view.SetIsGuest(userProfileBridge.GetOwn().isGuest);
            (IReadOnlyList<OutfitItem> outfits, int totalAmount) requestOwnedOutfits = await lambdaOutfitsService.RequestOwnedOutfits(userProfileBridge.GetOwn().userId, cancellationToken: ct);
            OutfitItem[] outfitItems = requestOwnedOutfits.outfits.ToArray();
            AudioScriptableObjects.listItemAppear.ResetPitch();
            view.SetSlotsAsLoading(outfitItems);

            foreach (OutfitItem outfitItem in outfitItems)
            {
                localOutfits[outfitItem.slot] = outfitItem;
                await view.ShowOutfit(outfitItem, GenerateAvatarModel(outfitItem), ct);
            }
        }

        private AvatarModel GenerateAvatarModel(OutfitItem outfitItem)
        {
            AvatarModel avatarModel = new AvatarModel();
            avatarModel.CopyFrom(currentAvatarModel);
            avatarModel.bodyShape = outfitItem.outfit.bodyShape;
            avatarModel.wearables = new List<string>(outfitItem.outfit.wearables.ToList());
            avatarModel.eyeColor = outfitItem.outfit.eyes.color;
            avatarModel.hairColor = outfitItem.outfit.hair.color;
            avatarModel.skinColor = outfitItem.outfit.skin.color;
            avatarModel.forceRender = new HashSet<string>(outfitItem.outfit.forceRender);
            return avatarModel;
        }

        public void UpdateAvatarPreview(AvatarModel newAvatarModel) =>
            currentAvatarModel = newAvatarModel;

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            cts = null;
            ctsShowOutfit.SafeCancelAndDispose();
            ctsShowOutfit = null;
            view.OnOutfitEquipped -= OutfitEquip;
            view.OnOutfitDiscarded -= DiscardOutfit;
            view.OnTrySaveAsGuest -= ShowGuestModal;

            dataStore.HUDs.avatarEditorVisible.OnChange -= ChangedVisibility;
        }
    }
}
