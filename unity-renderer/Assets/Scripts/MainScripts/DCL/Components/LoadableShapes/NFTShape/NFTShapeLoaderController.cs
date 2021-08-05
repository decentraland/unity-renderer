﻿using DCL.Components;
using DCL.Helpers.NFT;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using DCL;
using Newtonsoft.Json;
using NFTShape_Internal;
using UnityGLTF.Loader;

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

    public NFTShapeConfig config;
    public MeshRenderer meshRenderer;
    public new BoxCollider collider;
    public Color backgroundColor;
    public GameObject spinner;

    [HideInInspector] public bool alreadyLoadedAsset = false;
    [HideInInspector] public INFTInfoFetcher nftInfoFetcher;

    public event System.Action OnLoadingAssetSuccess;
    public event System.Action OnLoadingAssetFail;

    [SerializeField]
    NFTShapeMaterial[] materials;

    [Header("Noise Shader")]
    [SerializeField]
    NoiseType noiseType = NoiseType.Simplex;

    [SerializeField] bool noiseIs3D = false;
    [SerializeField] bool noiseIsFractal = false;

    System.Action<LoadWrapper> OnSuccess;
    System.Action<LoadWrapper> OnFail;
    private string darURLProtocol;
    private string darURLRegistry;
    private string darURLAsset;
    private IPromiseLike_TextureAsset assetPromise = null;

    public Material frameMaterial { private set; get; } = null;
    public Material imageMaterial { private set; get; } = null;
    public Material backgroundMaterial { private set; get; } = null;

    int BASEMAP_SHADER_PROPERTY = Shader.PropertyToID("_BaseMap");
    int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_BaseColor");

    GifPlayer gifPlayer = null;
    NFTShapeHQImageHandler hqTextureHandler = null;

    bool isDestroyed = false;

    private Coroutine fetchNftImage;

    void Awake()
    {
        Material[] meshMaterials = new Material[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            switch (materials[i].type)
            {
                case NFTShapeMaterial.MaterialType.BACKGROUND:
                    backgroundMaterial = new Material(materials[i].material);
                    meshMaterials[i] = backgroundMaterial;
                    break;
                case NFTShapeMaterial.MaterialType.FRAME:
                    frameMaterial = materials[i].material;
                    meshMaterials[i] = frameMaterial;
                    break;
                case NFTShapeMaterial.MaterialType.IMAGE:
                    imageMaterial = new Material(materials[i].material);
                    meshMaterials[i] = imageMaterial;
                    break;
            }
        }
        meshRenderer.materials = meshMaterials;

        // NOTE: we use half scale to keep backward compatibility cause we are using 512px to normalize the scale with a 256px value that comes from the images
        meshRenderer.transform.localScale = new Vector3(0.5f, 0.5f, 1);

        InitializePerlinNoise();
        nftInfoFetcher = new NFTInfoFetcher();
    }

    void Update() { hqTextureHandler?.Update(); }

    public void LoadAsset(string url, bool loadEvenIfAlreadyLoaded = false)
    {
        if (string.IsNullOrEmpty(url) || (!loadEvenIfAlreadyLoaded && alreadyLoadedAsset))
            return;

        UpdateBackgroundColor(backgroundColor);

        // Check the src follows the needed format e.g.: 'ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'
        var regexMatches = Regex.Matches(url, "(?<protocol>[^:]+)://(?<registry>0x([A-Fa-f0-9])+)(?:/(?<asset>.+))?");
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

        alreadyLoadedAsset = false;

        FetchNFTImage();
    }

    public void UpdateBackgroundColor(Color newColor)
    {
        if (backgroundMaterial == null)
            return;

        backgroundMaterial.SetColor(COLOR_SHADER_PROPERTY, newColor);
    }

    private void FetchNFTImage()
    {
        if (spinner != null)
            spinner.SetActive(true);
        nftInfoFetcher.FetchNFTImage(darURLProtocol, darURLRegistry, NFTInfoFeteched, NFTInfoFetchedFail);
    }

    private void NFTInfoFeteched(NFTInfo nftInfo) { fetchNftImage = StartCoroutine(FetchNFTImageCoroutine(nftInfo)); }

    private void NFTInfoFetchedFail() { FinishLoading(false); }

    private void FetchNFTInfoSuccess(ITexture asset, NFTInfo info, bool isPreview)
    {
        SetFrameImage(asset, resizeFrameMesh: true);
        SetupGifPlayer(asset);

        if (isPreview)
        {
            var hqImageHandlerConfig = new NFTShapeHQImageConfig()
            {
                controller = this,
                nftConfig = config,
                nftInfo = info,
                asset = NFTAssetFactory.CreateAsset(asset, config, UpdateTexture, gifPlayer)
            };
            hqTextureHandler = NFTShapeHQImageHandler.Create(hqImageHandlerConfig);
        }

        FinishLoading(true);
    }

    void FinishLoading(bool loadedSuccessfully)
    {
        if (loadedSuccessfully)
        {
            if (spinner != null)
                spinner.SetActive(false);

            OnLoadingAssetSuccess?.Invoke();
        }
        else
        {
            OnLoadingAssetFail?.Invoke();
        }
    }

    IEnumerator FetchNFTImageCoroutine(NFTInfo nftInfo)
    {
        bool isError = false;

        yield return new DCL.WaitUntil(() => (CommonScriptableObjects.playerUnityPosition - transform.position).sqrMagnitude < (config.loadingMinDistance * config.loadingMinDistance));

        // We download the "preview" 256px image
        bool foundDCLImage = false;
        if (!string.IsNullOrEmpty(nftInfo.previewImageUrl))
        {
            yield return WrappedTextureUtils.Fetch(nftInfo.previewImageUrl, (promise) =>
            {
                foundDCLImage = true;
                this.assetPromise = promise;
                FetchNFTInfoSuccess(promise.asset, nftInfo, true);
            });
        }

        //We fall back to the nft original image which can have a really big size
        if (!foundDCLImage && !string.IsNullOrEmpty(nftInfo.originalImageUrl))
        {
            yield return WrappedTextureUtils.Fetch(nftInfo.originalImageUrl,
                (promise) =>
                {
                    foundDCLImage = true;
                    this.assetPromise = promise;
                    FetchNFTInfoSuccess(promise.asset, nftInfo, false);
                }, () => isError = true);
        }

        if (isError)
        {
            Debug.LogError($"Couldn't fetch NFT image for: '{darURLRegistry}/{darURLAsset}' {nftInfo.originalImageUrl}");
            FinishLoading(false);
        }
    }

    void SetFrameImage(ITexture newAsset, bool resizeFrameMesh = false)
    {
        if (newAsset == null)
            return;

        UpdateTexture(newAsset.texture);

        if (resizeFrameMesh && !isDestroyed && meshRenderer != null)
        {
            float w, h;
            w = h = 0.5f;
            if (newAsset.width > newAsset.height)
                h *= newAsset.height / (float)newAsset.width;
            else if (newAsset.width < newAsset.height)
                w *= newAsset.width / (float)newAsset.height;
            Vector3 newScale = new Vector3(w, h, 1f);

            meshRenderer.transform.localScale = newScale;
        }
    }

    public void UpdateTexture(Texture2D texture)
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
        isDestroyed = true;

        nftInfoFetcher.Dispose();

        if (fetchNftImage != null)
            StopCoroutine(fetchNftImage);
        if (assetPromise != null)
        {
            assetPromise.Forget();
            assetPromise = null;
        }

        if (hqTextureHandler != null)
        {
            hqTextureHandler.Dispose();
            hqTextureHandler = null;
        }

        if (gifPlayer != null)
        {
            gifPlayer.OnFrameTextureChanged -= UpdateTexture;
            gifPlayer.Dispose();
        }

        if (backgroundMaterial != null)
        {
            Object.Destroy(backgroundMaterial);
        }
        if (imageMaterial != null)
        {
            Object.Destroy(imageMaterial);
        }
    }

    private void SetupGifPlayer(ITexture asset)
    {
        if (!(asset is Asset_Gif gifAsset))
        {
            return;
        }

        gifPlayer = new GifPlayer(gifAsset);
        gifPlayer.Play();
        gifPlayer.OnFrameTextureChanged += UpdateTexture;
    }
}
