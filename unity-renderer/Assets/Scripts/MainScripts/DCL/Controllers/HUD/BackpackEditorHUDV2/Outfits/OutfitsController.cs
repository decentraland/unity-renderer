using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class OutfitsController : IDisposable
{
    private readonly LambdaOutfitsService lambdaOutfitsService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly OutfitsSectionComponentView view;
    private readonly IWearablesCatalogService wearablesCatalogService;
    public event Action<OutfitItem> OnOutfitEquipped;

    private CancellationTokenSource cts;
    private OutfitItem[] localOutfits;
    private bool shouldDeploy;

    public OutfitsController(
        OutfitsSectionComponentView view,
        LambdaOutfitsService lambdaOutfitsService,
        IUserProfileBridge userProfileBridge,
        IWearablesCatalogService wearablesCatalogService,
        DataStore dataStore)
    {
        this.view = view;
        this.lambdaOutfitsService = lambdaOutfitsService;
        this.userProfileBridge = userProfileBridge;
        this.wearablesCatalogService = wearablesCatalogService;

        view.OnOutfitEquipped += (outfit)=>OnOutfitEquipped?.Invoke(outfit);
        view.OnUpdateLocalOutfits += UpdateLocalOutfits;

        dataStore.HUDs.avatarEditorVisible.OnChange += ChangedVisibility;
    }

    private void ChangedVisibility(bool current, bool previous)
    {
        if(shouldDeploy)
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
    }
}
