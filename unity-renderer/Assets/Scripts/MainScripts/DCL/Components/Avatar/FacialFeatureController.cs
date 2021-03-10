using DCL.Helpers;
using System.Collections;
using System.Linq;
using UnityEngine;
using DCL;

public class FacialFeatureController
{
    public bool isReady { get; private set; }
    public string wearableId => wearableItem?.id;

    private readonly WearableItem wearableItem;
    private readonly Material baseMaterial;

    internal Texture mainTexture = null;
    internal Texture maskTexture = null;
    internal AssetPromise_Texture mainTexturePromise = null;
    internal AssetPromise_Texture maskTexturePromise = null;

    private Color color;
    private IBodyShapeController bodyShape;
    internal Material baseMaterialCopy;

    public FacialFeatureController(WearableItem wearableItem, Material baseMaterial)
    {
        isReady = false;
        this.baseMaterial = baseMaterial;
        this.wearableItem = wearableItem;
    }

    public void Load(IBodyShapeController loadedBody, Color color)
    {
        this.color = color;

        if (isReady)
        {
            PrepareWearable();
            return;
        }

        this.bodyShape = loadedBody;
        CoroutineStarter.Start(FetchTextures());
    }

    void PrepareWearable()
    {

        if (baseMaterialCopy == null)
            baseMaterialCopy = new Material(baseMaterial);

        switch (wearableItem.category)
        {
            case WearableLiterals.Categories.EYES:
                bodyShape.SetupEyes(baseMaterialCopy, mainTexture, maskTexture, color);
                break;
            case WearableLiterals.Categories.EYEBROWS:
                bodyShape.SetupEyebrows(baseMaterialCopy, mainTexture, color);
                break;
            case WearableLiterals.Categories.MOUTH:
                bodyShape.SetupMouth(baseMaterialCopy, mainTexture, maskTexture, color);
                break;
        }

        isReady = true;
    }

    public IEnumerator FetchTextures()
    {
        if (mainTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);

        if (maskTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);

        mainTexture = null;
        maskTexture = null;

        var representation = wearableItem.GetRepresentation(bodyShape.bodyShapeId);

        string mainTextureHash = representation?.contents?.FirstOrDefault(x => x.file == representation?.mainFile)?.hash;
        if (mainTextureHash == null)
            mainTextureHash = representation?.contents?.FirstOrDefault(x => !x.file.ToLower().Contains("_mask.png"))?.hash;
        string maskhash = representation?.contents?.FirstOrDefault(x => x.file.ToLower().Contains("_mask.png"))?.hash;

        if (!string.IsNullOrEmpty(mainTextureHash))
        {
            mainTexturePromise = new AssetPromise_Texture(wearableItem.baseUrl + mainTextureHash);
            mainTexturePromise.OnSuccessEvent += (x) => mainTexture = x.texture;
            mainTexturePromise.OnFailEvent += (x) => mainTexture = null;

            AssetPromiseKeeper_Texture.i.Keep(mainTexturePromise);
        }

        if (!string.IsNullOrEmpty(maskhash))
        {
            maskTexturePromise = new AssetPromise_Texture(wearableItem.baseUrl + maskhash);
            maskTexturePromise.OnSuccessEvent += (x) => maskTexture = x.texture;
            maskTexturePromise.OnFailEvent += (x) => maskTexture = null;

            AssetPromiseKeeper_Texture.i.Keep(maskTexturePromise);
        }

        yield return mainTexturePromise;
        yield return maskTexturePromise;

        PrepareWearable();
    }

    public void CleanUp()
    {
        if (mainTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);

        if (maskTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);

        Object.Destroy(baseMaterialCopy);

        isReady = false;
    }

    public static FacialFeatureController CreateDefaultFacialFeature(string bodyShape, string category, Material material)
    {
        string defaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, category);
        CatalogController.wearableCatalog.TryGetValue(defaultId, out WearableItem wearable);
        if (wearable == null)
        {
            Debug.LogError($"Couldn't resolve wearable {defaultId}");
            return null;
        }

        return new FacialFeatureController(wearable, material);
    }
}