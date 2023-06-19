using Cysharp.Threading.Tasks;
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

    public OutfitsController(OutfitsSectionComponentView view, LambdaOutfitsService lambdaOutfitsService, IUserProfileBridge userProfileBridge, IWearablesCatalogService wearablesCatalogService)
    {
        this.view = view;
        this.lambdaOutfitsService = lambdaOutfitsService;
        this.userProfileBridge = userProfileBridge;
        this.wearablesCatalogService = wearablesCatalogService;
        view.OnOutfitEquipped += (outfit)=>OnOutfitEquipped?.Invoke(outfit);
        view.OnSaveOutfits += SaveOutfits;
    }

    private void SaveOutfits(OutfitItem[] outfits) =>
        WebInterface.SaveUserOutfits(outfits);

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
        view.OnSaveOutfits -= SaveOutfits;
    }
}
