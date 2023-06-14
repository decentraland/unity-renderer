using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class OutfitsController : IDisposable
{
    private readonly LambdaOutfitsService lambdaOutfitsService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly OutfitsSectionComponentView view;
    public event Action<OutfitItem> OnOutfitEquipped;

    private CancellationTokenSource cts;

    public OutfitsController(OutfitsSectionComponentView view, LambdaOutfitsService lambdaOutfitsService, IUserProfileBridge userProfileBridge)
    {
        this.view = view;
        this.lambdaOutfitsService = lambdaOutfitsService;
        this.userProfileBridge = userProfileBridge;
        view.OnOutfitEquipped += (outfit)=>OnOutfitEquipped?.Invoke(outfit);
    }

    public void RequestOwnedOutfits()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        //lambdaOutfitsService.RequestOwnedOutfits(userProfileBridge.GetOwn().userId, cancellationToken: cts.Token).Forget();
    }

    public void UpdateAvatarPreview(AvatarModel newAvatarModel) =>
        view.UpdateAvatarPreview(newAvatarModel);

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
