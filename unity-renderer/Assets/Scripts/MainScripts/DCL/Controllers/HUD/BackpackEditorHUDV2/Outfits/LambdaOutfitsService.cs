using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.Lambdas;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LambdaOutfitsService
{
    private const string OUTFITS_ENDPOINT = "outfits/";
    private readonly ILambdasService lambdasService;
    private ICatalyst catalyst;

    public LambdaOutfitsService(ILambdasService lambdasService, IServiceProviders serviceProviders)
    {
        this.lambdasService = lambdasService;
        this.catalyst = serviceProviders.catalyst;
    }

    public async UniTask<(IReadOnlyList<OutfitItem> outfits, int totalAmount)> RequestOwnedOutfits(
        string userId,
        CancellationToken cancellationToken)
    {
        await UniTask.WaitUntil(() => catalyst.lambdasUrl != null, cancellationToken: cancellationToken);
        //webRequestController.Ref.Get(url, requestAttemps: attemptsNumber, timeout: timeout, disposeOnCompleted: false);
        (OutfitsResponse response, bool success) = await lambdasService.Get<OutfitsResponse>(
            OUTFITS_ENDPOINT + userId,
            OUTFITS_ENDPOINT + userId,
            cancellationToken: cancellationToken);

        if (!success)
            throw new Exception($"The request of outfits for '{userId}' failed!");

        return (response.elements, response.elements.Count);
    }
}