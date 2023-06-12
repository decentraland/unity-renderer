using Cysharp.Threading.Tasks;
using DCLServices.Lambdas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OutfitsController : IDisposable
{
    private readonly LambdaOutfitsService lambdaOutfitsService;
    private readonly IUserProfileBridge userProfileBridge;

    private CancellationTokenSource cts;

    public OutfitsController(LambdaOutfitsService lambdaOutfitsService, IUserProfileBridge userProfileBridge)
    {
        this.lambdaOutfitsService = lambdaOutfitsService;
        this.userProfileBridge = userProfileBridge;
    }

    public void RequestOwnedOutfits()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        lambdaOutfitsService.RequestOwnedOutfits(userProfileBridge.GetOwn().userId, cancellationToken: cts.Token).Forget();
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
