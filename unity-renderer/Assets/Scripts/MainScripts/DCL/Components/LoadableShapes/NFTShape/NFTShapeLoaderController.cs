using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Helpers.NFT;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class NFTShapeLoaderController : MonoBehaviour
{
    public enum NoiseType
    {
        ClassicPerlin,
        PeriodicPerlin,
        Simplex,
        SimplexNumericalGrad,
        SimplexAnalyticalGrad,
        None
    }

    public MeshRenderer meshRenderer;
    public new BoxCollider collider;
    public Color backgroundColor;

    [HideInInspector] public bool alreadyLoadedAsset = false;

    public event System.Action OnLoadingAssetSuccess;
    public event System.Action OnLoadingAssetFail;

    [Header("Material Indexes")]
    [SerializeField] int materialIndex_Background = -1;
    [SerializeField] int materialIndex_NFTImage = -1;
    [SerializeField] int materialIndex_Frame = -1;

    [Header("Noise Shader")]
    [SerializeField] NoiseType noiseType = NoiseType.Simplex;
    [SerializeField] bool noiseIs3D = false;
    [SerializeField] bool noiseIsFractal = false;

    System.Action<LoadWrapper> OnSuccess;
    System.Action<LoadWrapper> OnFail;
    string darURLProtocol;
    string darURLRegistry;
    string darURLAsset;

    Material frameMaterial = null;
    Material imageMaterial = null;
    Material backgroundMaterial = null;

    int BASEMAP_SHADER_PROPERTY = Shader.PropertyToID("_BaseMap");
    int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_BaseColor");

    DCL.IWrappedTextureAsset nftAsset;

    bool VERBOSE = false;

    void Awake()
    {
        if (materialIndex_NFTImage >= 0) imageMaterial = meshRenderer.materials[materialIndex_NFTImage];
        if (materialIndex_Background >= 0) backgroundMaterial = meshRenderer.materials[materialIndex_Background];
        if (materialIndex_Frame >= 0) frameMaterial = meshRenderer.materials[materialIndex_Frame];

        InitializePerlinNoise();
    }

    public void LoadAsset(string url = "", bool loadEvenIfAlreadyLoaded = false)
    {
        if (string.IsNullOrEmpty(url) || (!loadEvenIfAlreadyLoaded && alreadyLoadedAsset)) return;

        UpdateBackgroundColor(backgroundColor);

        // Check the src follows the needed format e.g.: 'ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'
        var regexMatches = Regex.Matches(url, "(?<protocol>[^:]+)://(?<registry>[^/]+)(?:/(?<asset>.+))?");
        if (regexMatches.Count == 0)
        {
            Debug.LogError($"Couldn't fetch DAR url '{url}' for NFTShape. The accepted format is 'ethereum://ContractAddress/TokenID'");

            OnLoadingAssetFail?.Invoke();

            return;
        }

        Match match = regexMatches[0];
        if (match.Groups["protocol"] == null || match.Groups["registry"] == null || match.Groups["asset"] == null)
        {
            Debug.LogError($"Couldn't fetch DAR url '{url}' for NFTShape.");

            OnLoadingAssetFail?.Invoke();

            return;
        }

        darURLProtocol = match.Groups["protocol"].ToString();
        if (darURLProtocol != "ethereum")
        {
            Debug.LogError($"Couldn't fetch DAR url '{url}' for NFTShape. The only protocol currently supported is 'ethereum'");

            OnLoadingAssetFail?.Invoke();

            return;
        }

        darURLRegistry = match.Groups["registry"].ToString();
        darURLAsset = match.Groups["asset"].ToString();

        if (VERBOSE)
        {
            Debug.Log("protocol: " + darURLProtocol);
            Debug.Log("registry: " + darURLRegistry);
            Debug.Log("asset: " + darURLAsset);
        }

        alreadyLoadedAsset = false;

        StartCoroutine(FetchNFTImage());
    }

    public void UpdateBackgroundColor(Color newColor)
    {
        if (backgroundMaterial == null)
            return;

        backgroundMaterial.SetColor(COLOR_SHADER_PROPERTY, newColor);
    }

    IEnumerator FetchNFTImage()
    {
        string thumbnailImageURL = null;
        string previewImageURL = null;
        string originalImageURL = null;

        yield return NFTHelper.FetchNFTInfo(darURLRegistry, darURLAsset,
            (nftInfo) =>
            {
                thumbnailImageURL = nftInfo.thumbnailUrl;
                previewImageURL = nftInfo.previewImageUrl;
                originalImageURL = nftInfo.originalImageUrl;
            },
            (error) =>
            {
                Debug.LogError($"Didn't find any asset image for '{darURLRegistry}/{darURLAsset}' for the NFTShape.\n{error}");
            });

        // We fetch and show the thumbnail image first
        if (!string.IsNullOrEmpty(thumbnailImageURL))
        {
            yield return Utils.FetchWrappedTextureAsset(thumbnailImageURL, (downloadedAsset) =>
            {
                SetFrameImage(downloadedAsset);
            });
        }

        // We fetch the final image
        bool foundDCLImage = false;
        if (!string.IsNullOrEmpty(previewImageURL))
        {
            yield return Utils.FetchWrappedTextureAsset(previewImageURL, (downloadedAsset) =>
            {
                // Dispose thumbnail
                if (nftAsset != null) nftAsset.Dispose();
                foundDCLImage = true;
                SetFrameImage(downloadedAsset, resizeFrameMesh: true);
            });
        }

        // We fall back to a common "preview" image if no "dcl image" was found
        if (!foundDCLImage && !string.IsNullOrEmpty(originalImageURL))
        {
            yield return Utils.FetchWrappedTextureAsset(originalImageURL, (downloadedAsset) =>
            {
                // Dispose thumbnail
                if (nftAsset != null) nftAsset.Dispose();
                SetFrameImage(downloadedAsset, resizeFrameMesh: true);
            });
        }

        OnLoadingAssetSuccess?.Invoke();
    }

    void SetFrameImage(DCL.IWrappedTextureAsset newAsset, bool resizeFrameMesh = false)
    {
        if (newAsset == null) return;

        nftAsset = newAsset;

        var gifAsset = nftAsset as DCL.WrappedGif;
        if (gifAsset != null)
        {
            gifAsset.SetUpdateTextureCallback(UpdateTexture);
        }

        UpdateTexture(nftAsset.texture);

        if (resizeFrameMesh)
        {
            Vector3 newScale = new Vector3(newAsset.width / NFTDataFetchingSettings.NORMALIZED_DIMENSIONS.x,
                                            newAsset.height / NFTDataFetchingSettings.NORMALIZED_DIMENSIONS.y, 1f);

            meshRenderer.transform.localScale = newScale;
        }
        else
        {
            meshRenderer.transform.localScale = Vector3.one;
        }
    }

    void UpdateTexture(Texture2D texture)
    {
        if (imageMaterial == null)
            return;

        imageMaterial.SetTexture(BASEMAP_SHADER_PROPERTY, texture);
        imageMaterial.SetColor(COLOR_SHADER_PROPERTY, Color.white);
    }

    void InitializePerlinNoise()
    {
        if (frameMaterial == null)
            return;

        frameMaterial.shaderKeywords = null;

        if (noiseType == NoiseType.None)
            return;

        switch (noiseType)
        {
            case NoiseType.ClassicPerlin:
                frameMaterial.EnableKeyword("CNOISE");
                break;
            case NoiseType.PeriodicPerlin:
                frameMaterial.EnableKeyword("PNOISE");
                break;
            case NoiseType.Simplex:
                frameMaterial.EnableKeyword("SNOISE");
                break;
            case NoiseType.SimplexNumericalGrad:
                frameMaterial.EnableKeyword("SNOISE_NGRAD");
                break;
            default: // SimplexAnalyticalGrad
                frameMaterial.EnableKeyword("SNOISE_AGRAD");
                break;
        }

        if (noiseIs3D)
            frameMaterial.EnableKeyword("THREED");

        if (noiseIsFractal)
            frameMaterial.EnableKeyword("FRACTAL");
    }

    void OnDestroy()
    {
        if (nftAsset != null)
        {
            nftAsset.Dispose();
        }
    }
}
