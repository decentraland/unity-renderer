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

        (OutfitsResponse response, bool success) = await lambdasService.Get<OutfitsResponse>(
            OUTFITS_ENDPOINT + userId,
            OUTFITS_ENDPOINT + userId,
            cancellationToken: cancellationToken);

        foreach (OutfitItem responseElement in response.metadata.outfits)
        {
            //Debug.Log(responseElement.slot + " " + responseElement.outfit.bodyShape + " " +  responseElement.outfit.eyes + " " +  responseElement.outfit.hair + " " +  responseElement.outfit.skin + " " +  responseElement.outfit.wearables.Length);
        }

        if (!success)
            throw new Exception($"The request of outfits for '{userId}' failed!");

        return (response.metadata.outfits, response.metadata.outfits.Length);
    }
}
