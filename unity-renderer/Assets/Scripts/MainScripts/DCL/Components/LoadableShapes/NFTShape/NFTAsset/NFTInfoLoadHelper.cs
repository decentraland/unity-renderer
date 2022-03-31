using System;
using System.Collections;
using DCL.Helpers.NFT;
using UnityEngine;

public interface INFTInfoLoadHelper : IDisposable
{
    public event Action<NFTInfo> OnFetchInfoSuccess;
    public event Action OnFetchInfoFail;
    void FetchNFTInfo(string address, string id);
}

public class NFTInfoLoadHelper : INFTInfoLoadHelper
{
    public event Action<NFTInfo> OnFetchInfoSuccess;
    public event Action OnFetchInfoFail;
    internal Coroutine fetchCoroutine;

    public void Dispose()
    {
        if (fetchCoroutine != null)
            CoroutineStarter.Stop(fetchCoroutine);

        fetchCoroutine = null;
    }

    public void FetchNFTInfo(string address, string id)
    {
        if (fetchCoroutine != null)
            CoroutineStarter.Stop(fetchCoroutine);

        fetchCoroutine = CoroutineStarter.Start(FetchNFTInfoCoroutine(address, id));
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