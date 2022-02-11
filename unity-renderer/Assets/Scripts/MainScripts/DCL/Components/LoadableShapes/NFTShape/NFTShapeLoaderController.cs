using DCL.Components;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using DCL;
using Newtonsoft.Json;
using NFTShape_Internal;
using UnityGLTF.Loader;

public class NFTShapeLoaderController : MonoBehaviour
{
    internal const string COULD_NOT_FETCH_DAR_URL = "Couldn't fetch DAR url '{0}' for NFTShape.";
    internal const string ACCEPTED_URL_FORMAT = "The accepted format is 'ethereum://ContractAddress/TokenID'.";
    internal const string SUPPORTED_PROTOCOL = "The only protocol currently supported is 'ethereum'.";
    internal const string DOES_NOT_SUPPORT_POLYGON = "Warning: OpenSea API does not support fetching Polygon assets.";
    internal const string COULD_NOT_FETCH_NFT_FROM_API = "Couldn't fetch NFT: '{0}/{1}'.";
    internal const string COULD_NOT_FETCH_NFT_IMAGE = "Couldn't fetch NFT image for: '{0}/{1}': {2}.";

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
    public GameObject errorFeedback;

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

    private AssetPromise_Texture imagePromise = null;
    private AssetPromise_Gif gifPromise = null;

    public Material frameMaterial { private set; get; } = null;
    public Material imageMaterial { private set; get; } = null;
    public Material backgroundMaterial { private set; get; } = null;

    int BASEMAP_SHADER_PROPERTY = Shader.PropertyToID("_BaseMap");
    int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_BaseColor");

    GifPlayer gifPlayer = null;
    NFTShapeHQImageHandler hqTextureHandler = null;

    bool isDestroyed = false;

    private Coroutine fetchNftImageCoroutine;
    internal IWrappedTextureHelper wrappedTextureHelper;

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
        wrappedTextureHelper = new WrappedTextureUtils();
    }

    private void Start() { spinner.layer = LayerMask.NameToLayer("ViewportCullingIgnored"); }

    void Update() { hqTextureHandler?.Update(); }

    public void LoadAsset(string url, bool loadEvenIfAlreadyLoaded = false)
    {
        if (string.IsNullOrEmpty(url) || (!loadEvenIfAlreadyLoaded && alreadyLoadedAsset))
            return;

        ShowErrorFeedback(false);
        UpdateBackgroundColor(backgroundColor);

        // Check the src follows the needed format e.g.: 'ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'
        var regexMatches = Regex.Matches(url, "(?<protocol>[^:]+)://(?<registry>0x([A-Fa-f0-9])+)(?:/(?<asset>.+))?");
        if (regexMatches.Count == 0)
        {
            Debug.LogError(string.Format(COULD_NOT_FETCH_DAR_URL + " " + ACCEPTED_URL_FORMAT, url));
            ShowErrorFeedback(true);
            OnLoadingAssetFail?.Invoke();

            return;
        }

        Match match = regexMatches[0];
        if (match.Groups["protocol"] == null || match.Groups["registry"] == null || match.Groups["asset"] == null)
        {
            Debug.LogError(string.Format(COULD_NOT_FETCH_DAR_URL + " " + ACCEPTED_URL_FORMAT, url));
            ShowErrorFeedback(true);
            OnLoadingAssetFail?.Invoke();

            return;
        }

        darURLProtocol = match.Groups["protocol"].ToString();
        if (darURLProtocol != "ethereum")
        {
            Debug.LogError(string.Format(COULD_NOT_FETCH_DAR_URL + " " + SUPPORTED_PROTOCOL + " " + ACCEPTED_URL_FORMAT, url));
            ShowErrorFeedback(true);
            OnLoadingAssetFail?.Invoke();

            return;
        }

        darURLRegistry = match.Groups["registry"].ToString();
        darURLAsset = match.Groups["asset"].ToString();

        alreadyLoadedAsset = false;

        FetchNFTContents();
    }

    public void UpdateBackgroundColor(Color newColor)
    {
        if (backgroundMaterial == null)
            return;

        backgroundMaterial.SetColor(COLOR_SHADER_PROPERTY, newColor);
    }

    private void FetchNFTContents()
    {
        ShowLoading(true);
        nftInfoFetcher.FetchNFTImage(darURLRegistry, darURLAsset, FetchNFTContentsSuccess, FetchNFTContentsFail);
    }

    private void FetchNFTContentsSuccess(NFTInfo nftInfo)
    {
        fetchNftImageCoroutine = StartCoroutine(FetchNFTImageCoroutine(nftInfo));
    }

    private void FetchNFTContentsFail()
    {
        ShowErrorFeedback(true);
        FinishLoading(false);
    }

    private void PrepareFrame(Texture2D previewTexture, INFTAsset nftAsset, string nftName, string nftImageUrl)
    {
        SetFrameImage(previewTexture, resizeFrameMesh: true);

        var hqImageHandlerConfig = new NFTShapeHQImageConfig()
        {
            controller = this,
            config = config,
            name = nftName,
            imageUrl = nftImageUrl,
            asset = nftAsset
        };

        hqTextureHandler = NFTShapeHQImageHandler.Create(hqImageHandlerConfig);
    }

    private IEnumerator FetchNFTImageInternal(string url, string nftName, System.Action<bool> OnFinished)
    {
        if (string.IsNullOrEmpty(url))
            yield break;

        string contentType = null;
        HashSet<string> headers = new HashSet<string>() {"Content-Type", "Content-Length"};
        Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

        yield return wrappedTextureHelper.GetHeaders(url, headers, result => responseHeaders = result, null);

        contentType = responseHeaders["Content-Type"];
        long contentLength = long.Parse(responseHeaders["Content-Length"]);

        bool isGif = contentType == "image/gif";

        if (isGif)
        {
            yield return wrappedTextureHelper.FetchGif(url, (promise) =>
            {
                UnloadContents();
                this.gifPromise = promise;
                SetupGifPlayer(promise.asset);
                var nftAsset = NFTAssetFactory.CreateGifAsset(promise.asset, config, gifPlayer);
                PrepareFrame(promise.asset.texture, nftAsset, nftName, url);
                OnFinished?.Invoke(true);
            });
        }
        else
        {
            const long PREVIEW_IMAGE_SIZE_LIMIT = 500000;

            if (contentLength > PREVIEW_IMAGE_SIZE_LIMIT)
            {
                OnFinished?.Invoke(false);
                yield break;
            }

            yield return wrappedTextureHelper.FetchImage(url, (promise) =>
            {
                UnloadContents();
                this.imagePromise = promise;
                var nftAsset = NFTAssetFactory.CreateImageAsset(promise.asset, config, UpdateTexture);
                PrepareFrame(promise.asset.texture, nftAsset, nftName, url);
                OnFinished?.Invoke(true);
            });
        }

        OnFinished?.Invoke(false);
    }

    void UnloadContents()
    {
        if (gifPromise != null)
            AssetPromiseKeeper_Gif.i.Forget(gifPromise);

        if (imagePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(imagePromise);

        gifPromise = null;
        imagePromise = null;
    }

    internal IEnumerator FetchNFTImageCoroutine(NFTInfo nftInfo)
    {
        yield return new DCL.WaitUntil(() => (CommonScriptableObjects.playerUnityPosition - transform.position).sqrMagnitude < (config.loadingMinDistance * config.loadingMinDistance));

        bool success = false;

        // We download the "preview" 256px image
        if (!string.IsNullOrEmpty(nftInfo.previewImageUrl))
        {
            yield return FetchNFTImageInternal(nftInfo.previewImageUrl, nftInfo.name, (result) => success = result);
        }

        if (!success)
        {
            Debug.LogError(string.Format(COULD_NOT_FETCH_NFT_IMAGE, darURLRegistry, darURLAsset,
                nftInfo.previewImageUrl));
            ShowErrorFeedback(true);
            OnLoadingAssetFail?.Invoke();
            yield break;
        }

        FinishLoading(success);
    }

    void FinishLoading(bool loadedSuccessfully)
    {
        if (loadedSuccessfully)
        {
            ShowLoading(false);
            OnLoadingAssetSuccess?.Invoke();
        }
        else
        {
            OnLoadingAssetFail?.Invoke();
        }
    }

    void SetFrameImage(Texture2D texture, bool resizeFrameMesh = false)
    {
        if (texture == null)
            return;

        UpdateTexture(texture);

        if (resizeFrameMesh && !isDestroyed && meshRenderer != null)
        {
            float w, h;
            w = h = 0.5f;
            if (texture.width > texture.height)
                h *= texture.height / (float) texture.width;
            else if (texture.width < texture.height)
                w *= texture.width / (float) texture.height;
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

    void OnDestroy()
    {
        isDestroyed = true;

        UnloadContents();

        nftInfoFetcher.Dispose();

        if (fetchNftImageCoroutine != null)
            StopCoroutine(fetchNftImageCoroutine);

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

    private void SetupGifPlayer(Asset_Gif asset)
    {
        gifPlayer = new GifPlayer(asset);
        gifPlayer.Play();
        gifPlayer.OnFrameTextureChanged += UpdateTexture;
    }

    private void ShowLoading(bool isVisible)
    {
        if (spinner == null)
            return;

        spinner.SetActive(isVisible);
    }

    private void ShowErrorFeedback(bool isVisible)
    {
        if (errorFeedback == null)
            return;

        if (isVisible)
            ShowLoading(false);

        errorFeedback.SetActive(isVisible);
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

}