using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FontLoaderAddressable : MonoBehaviour
{
    [SerializeField]
    private List<AssetReferenceT<TMP_FontAsset>> fontAssetReference;

    void Start()
    {
        Addressables.InitializeAsync().Completed += AddressablesInitiated;
    }

    private void AddressablesInitiated(AsyncOperationHandle<IResourceLocator> obj)
    {
        foreach (AssetReferenceT<TMP_FontAsset> assetReferenceT in fontAssetReference)
        {
            assetReferenceT.LoadAssetAsync().Completed += FontLoadComplete;
        }
    }

    private void FontLoadComplete(AsyncOperationHandle<TMP_FontAsset> obj)
    {

        var fallbackFontAssets = TMP_Settings.fallbackFontAssets;

        if (fallbackFontAssets == null)
        {
            fallbackFontAssets = new List<TMP_FontAsset>();
        }

        fallbackFontAssets.Add(obj.Result);

        Debug.Log($"Font {obj.Result.name} loaded successfully");

        TMP_Settings.fallbackFontAssets = fallbackFontAssets;
    }
}
