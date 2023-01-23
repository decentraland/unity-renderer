using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FontLoaderAddressable : MonoBehaviour
{

    private void Start()
    {
        Addressables.InitializeAsync().Completed += AddressablesInitiated;
    }

    private void AddressablesInitiated(AsyncOperationHandle<IResourceLocator> obj)
    {
        Addressables.LoadAssetsAsync<TMP_FontAsset>("fonts", FontLoaded);
    }

    private void FontLoaded(TMP_FontAsset obj)
    {
        List<TMP_FontAsset> fallbackFontAssets = TMP_Settings.fallbackFontAssets;

        if (fallbackFontAssets == null) { fallbackFontAssets = new List<TMP_FontAsset>(); }

        fallbackFontAssets.Add(obj);

        TMP_Settings.fallbackFontAssets = fallbackFontAssets;

        Debug.Log($"Font {obj.name} loaded succesfully");
    }

}


