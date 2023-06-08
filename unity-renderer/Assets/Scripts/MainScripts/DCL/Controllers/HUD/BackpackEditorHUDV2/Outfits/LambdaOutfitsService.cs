using Cysharp.Threading.Tasks;
using DCLServices.Lambdas;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LambdaOutfitsService : MonoBehaviour
{
    private const string OUTFITS_ENDPOINT = "outfits/";
    private readonly ILambdasService lambdasService;
    private ICatalyst catalyst;

    public async UniTask<(IReadOnlyList<OutfitItem> outfits, int totalAmount)> RequestOwnedOutfits(
        string userId,
        CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => catalyst.lambdasUrl != null, cancellationToken: cancellationToken);

            (OutfitsResponse response, bool success) = await lambdasService.GetFromSpecificUrl<OutfitsResponse>(
                OUTFITS_ENDPOINT + userId,
                OUTFITS_ENDPOINT + userId,
                cancellationToken: cancellationToken);

            if (!success)
                throw new Exception($"The request of outfits for '{userId}' failed!");

            return (response.elements, response.elements.Count);
        }
}
