using DCL.Components;
using DCL.Helpers.NFT;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using DCL.Controllers.Gif;
using DCL;
using NFTShape_Internal;

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
    string darURLProtocol;
    string darURLRegistry;
    string darURLAsset;

    public Material frameMaterial { private set; get; } = null;
    public Material imageMaterial { private set; get; } = null;
    public Material backgroundMaterial { private set; get; } = null;

    int BASEMAP_SHADER_PROPERTY = Shader.PropertyToID("_BaseMap");
    int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_BaseColor");

    DCL.ITexture nftAsset;
    AssetPromise_Texture texturePromise = null;
    NFTShapeHQImageHandler hqTextureHandler = null;

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
    }

    void Update()
    {
        hqTextureHandler?.Update();
    }

    public void LoadAsset(string url, bool loadEvenIfAlreadyLoaded = false)
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
        if (spinner != null)
            spinner.SetActive(true);

        NFTInfo nftInfo = new NFTInfo();

        bool isError = false;

        yield return NFTHelper.FetchNFTInfo(darURLRegistry, darURLAsset,
            (info) =>
            {
                nftInfo = info;
            },
            (error) =>
            {
                Debug.LogError($"Couldn't fetch NFT: '{darURLRegistry}/{darURLAsset}' {error}");
                OnLoadingAssetFail?.Invoke();
                isError = true;
            });

        if (isError)
            yield break;

        yield return new DCL.WaitUntil(() => (CommonScriptableObjects.playerUnityPosition - transform.position).sqrMagnitude < (config.loadingMinDistance * config.loadingMinDistance));

        // We download the "preview" 256px image
        bool foundDCLImage = false;
        if (!string.IsNullOrEmpty(nftInfo.previewImageUrl))
        {
            yield return WrappedTextureUtils.Fetch(nftInfo.previewImageUrl, (downloadedTex, texturePromise) =>
            {
                foundDCLImage = true;
                this.texturePromise = texturePromise;
                SetFrameImage(downloadedTex, resizeFrameMesh: true);

                var hqImageHandlerConfig = new NFTShapeHQImageConfig()
                {
                    controller = this,
                    nftConfig = config,
                    nftInfo = nftInfo,
                    asset = NFTAssetFactory.CreateAsset(downloadedTex, config)
                };
                hqTextureHandler = NFTShapeHQImageHandler.Create(hqImageHandlerConfig);
            });
        }

        //We fall back to the nft original image which can have a really big size
        if (!foundDCLImage && !string.IsNullOrEmpty(nftInfo.originalImageUrl))
        {
            yield return WrappedTextureUtils.Fetch(nftInfo.originalImageUrl,
                (downloadedTex, texturePromise) =>
                {
                    foundDCLImage = true;
                    this.texturePromise = texturePromise;
                    SetFrameImage(downloadedTex, resizeFrameMesh: true);
                }, () => isError = true);
        }

        if (isError)
        {
            Debug.LogError($"Couldn't fetch NFT image for: '{darURLRegistry}/{darURLAsset}' {nftInfo.originalImageUrl}");
            OnLoadingAssetFail?.Invoke();
            yield break;
        }

        FinishLoading(foundDCLImage);
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

    void SetFrameImage(DCL.ITexture newAsset, bool resizeFrameMesh = false)
    {
        if (newAsset == null) return;

        nftAsset = newAsset;

        if (nftAsset is Asset_Gif gifAsset)
        {
            gifAsset.OnFrameTextureChanged -= UpdateTexture;
            gifAsset.OnFrameTextureChanged += UpdateTexture;
            gifAsset.Play(false);
        }

        UpdateTexture(nftAsset.texture);

        if (resizeFrameMesh)
        {
            float w, h;
            w = h = 0.5f;
            if (newAsset.width > newAsset.height) h *= newAsset.height / (float)newAsset.width;
            else if (newAsset.width < newAsset.height) w *= newAsset.width / (float)newAsset.height;
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
        if (texturePromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(texturePromise);
            texturePromise = null;
        }
        else
        {
            nftAsset?.Dispose();
        }

        if (hqTextureHandler != null)
        {
            hqTextureHandler.Dispose();
            hqTextureHandler = null;
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
}