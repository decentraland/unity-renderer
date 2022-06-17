using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers.NFT;
using UnityEngine;

public interface INFTInfoRetriever : IDisposable
{
    public event Action<NFTInfo> OnFetchInfoSuccess;
    public event Action OnFetchInfoFail;
    void FetchNFTInfo(string address, string id);
    UniTask<NFTInfo> FetchNFTInfo(string src);
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

    public void FetchNFTInfo(string address, string id)
    {
        if (fetchCoroutine != null)
            CoroutineStarter.Stop(fetchCoroutine);

        fetchCoroutine = CoroutineStarter.Start(FetchNFTInfoCoroutine(address, id));
    }

    public async UniTask<NFTInfo> FetchNFTInfo(string src)
    {
        tokenSource = new CancellationTokenSource();
        tokenSource.Token.ThrowIfCancellationRequested();
        // Check the src follows the needed format e.g.: 'ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'
        var regexMatches = Regex.Matches(src, "(?<protocol>[^:]+)://(?<registry>0x([A-Fa-f0-9])+)(?:/(?<asset>.+))?");
        if (regexMatches.Count == 0 || regexMatches[0].Groups["protocol"] == null || regexMatches[0].Groups["registry"] == null || regexMatches[0].Groups["asset"] == null)
        {
            string errorMessage = string.Format(COULD_NOT_FETCH_DAR_URL + " " + ACCEPTED_URL_FORMAT, src);
            Debug.Log(errorMessage);
            OnFetchInfoFail?.Invoke();
            return null;
        }

        Match match = regexMatches[0];
        string darURLProtocol = match.Groups["protocol"].ToString();
        if (darURLProtocol != "ethereum")
        {
            string errorMessage = string.Format(COULD_NOT_FETCH_DAR_URL + " " + SUPPORTED_PROTOCOL + " " + ACCEPTED_URL_FORMAT, src);
            Debug.Log(errorMessage);
            OnFetchInfoFail?.Invoke();
            return null;
        }

        string darURLRegistry = match.Groups["registry"].ToString();
        string darURLAsset = match.Groups["asset"].ToString();

        NFTInfo nftInformation = null;
        
        var rutine = NFTUtils.FetchNFTInfo(darURLRegistry, darURLAsset,
            (info) =>
            {
                nftInformation = info;
            },
            (error) =>
            {
                Debug.LogError($"Couldn't fetch NFT: '{darURLRegistry}/{darURLAsset}' {error}");
            });
        
        await rutine.WithCancellation(tokenSource.Token);

        return nftInformation;
    }

    private IEnumerator FetchNFTInfoCoroutine(string address, string id)
    {
        yield return NFTUtils.FetchNFTInfo(address, id,
            (info) => { OnFetchInfoSuccess?.Invoke(info); },
            (error) =>
            {
                Debug.LogError($"Couldn't fetch NFT: '{address}/{id}' {error}");
                OnFetchInfoFail?.Invoke();
            });
    }
}