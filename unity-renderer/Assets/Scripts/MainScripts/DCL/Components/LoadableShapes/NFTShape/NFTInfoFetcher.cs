using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL;
using DCL.Helpers.NFT;
using Newtonsoft.Json;
using NFTShape_Internal;
using UnityEngine;
using UnityGLTF.Loader;

public interface INFTInfoFetcher
{
    void FetchNFTImage(string address, string id, Action<NFTInfo> OnSuccess, Action OnFail);
    void Dispose();
}

public class NFTInfoFetcher : INFTInfoFetcher
{
    internal Coroutine fetchCoroutine;

    public void Dispose()
    {
        if (fetchCoroutine != null)
            CoroutineStarter.Stop(fetchCoroutine);

        fetchCoroutine = null;
    }

    public void FetchNFTImage(string address, string id, Action<NFTInfo> OnSuccess, Action OnFail)
    {
        if (fetchCoroutine != null)
            CoroutineStarter.Stop(fetchCoroutine);

        fetchCoroutine = CoroutineStarter.Start(FetchNFTImageCoroutine(address, id, OnSuccess, OnFail));
    }

    private IEnumerator FetchNFTImageCoroutine(string address, string id, Action<NFTInfo> OnSuccess, Action OnFail)
    {
        yield return NFTUtils.FetchNFTInfo(address, id,
            (info) =>
            {
                OnSuccess?.Invoke(info);
            },
            (error) =>
            {
                Debug.LogError($"Couldn't fetch NFT: '{address}/{id}' {error}");
                OnFail?.Invoke();
            });
    }
}