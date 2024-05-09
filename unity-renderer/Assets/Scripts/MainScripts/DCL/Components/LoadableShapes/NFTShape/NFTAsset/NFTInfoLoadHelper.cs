using Cysharp.Threading.Tasks;
using DCL.Helpers.NFT;
using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public interface INFTInfoRetriever : IDisposable
{
    public event Action<NFTInfo> OnFetchInfoSuccess;
    public event Action OnFetchInfoFail;
    void FetchNFTInfo(string chain, string contractAddress, string tokenId);

    UniTask<NFTInfo?> FetchNFTInfoAsync(string chain, string contractAddress, string tokenId);
}

public class NFTInfoRetriever : INFTInfoRetriever
{
    internal const string COULD_NOT_FETCH_DAR_URL = "Couldn't fetch DAR url '{0}' for NFTShape.";
    internal const string ACCEPTED_URL_FORMAT = "The accepted format is 'ethereum://ContractAddress/TokenID'.";
    internal const string SUPPORTED_PROTOCOL = "The only protocol currently supported is 'ethereum'.";

    public event Action<NFTInfo> OnFetchInfoSuccess;
    public event Action OnFetchInfoFail;
    internal Coroutine fetchCoroutine;
    private CancellationTokenSource tokenSource;

    public void Dispose()
    {
        // Note: When we delete de old component NFTShape, we should remove the coroutine part
        if (fetchCoroutine != null)
            CoroutineStarter.Stop(fetchCoroutine);

        fetchCoroutine = null;
        tokenSource?.Cancel();
        tokenSource?.Dispose();
    }

    public void FetchNFTInfo(string chain, string contractAddress, string tokenId)
    {
        if (fetchCoroutine != null)
            CoroutineStarter.Stop(fetchCoroutine);

        fetchCoroutine = CoroutineStarter.Start(FetchNFTInfoCoroutine(chain, contractAddress, tokenId));
    }

    public async UniTask<NFTInfo?> FetchNFTInfoAsync(string chain, string contractAddress, string tokenId)
    {
        tokenSource = new CancellationTokenSource();
        tokenSource.Token.ThrowIfCancellationRequested();

        NFTInfo? nftInformation = null;

        var rutine = NFTUtils.FetchNFTInfo(chain, contractAddress, tokenId,
            (info) =>
            {
                nftInformation = info;
            },
            (error) =>
            {
                Debug.LogError($"Couldn't fetch NFT: '{contractAddress}/{tokenId}' {error}");
            });

        await rutine.WithCancellation(tokenSource.Token);
        return nftInformation;
    }

    private IEnumerator FetchNFTInfoCoroutine(string chain, string contractAddress, string tokenId)
    {
        yield return NFTUtils.FetchNFTInfo(chain, contractAddress, tokenId,
            (info) => { OnFetchInfoSuccess?.Invoke(info); },
            (error) =>
            {
                Debug.LogError($"Couldn't fetch NFT: '{contractAddress}/{tokenId}' {error}");
                OnFetchInfoFail?.Invoke();
            });
    }
}
