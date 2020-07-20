using DCL.Helpers;
using System.Collections;
using System.Linq;
using UnityEngine;
using DCL;

public class FacialFeatureController
{
    public string id => wearable.id;
    public string category => wearable.category;

    public bool isReady { get; private set; }

    private string bodyShapeType;
    public WearableItem wearable;

    Texture mainTexture = null;
    Texture maskTexture = null;
    AssetPromise_Texture mainTexturePromise = null;
    AssetPromise_Texture maskTexturePromise = null;

    private Color color;
    private BodyShapeController bodyShape;
    private Material baseMaterial;
    private Material baseMaterialCopy;

    public FacialFeatureController(WearableItem wearableItem, string bodyShapeType, Material baseMaterial)
    {
        isReady = false;
        this.baseMaterial = baseMaterial;
        this.wearable = wearableItem;
        this.bodyShapeType = bodyShapeType;
    }

    public void Load(BodyShapeController loadedBody, Color color)
    {
        this.color = color;

        if (isReady)
        {
            PrepareWearable();
            return;
        }

        this.bodyShape = loadedBody;
        CoroutineStarter.Start(FetchTextures(PrepareWearable));
    }

    void PrepareWearable()
    {
        if (baseMaterialCopy == null)
            baseMaterialCopy = new Material(baseMaterial);

        switch (wearable.category)
        {
            case WearableLiterals.Categories.EYES:
                bodyShape.SetupEyes(baseMaterialCopy, mainTexture, maskTexture, color);
                break;
            case WearableLiterals.Categories.EYEBROWS:
                bodyShape.SetupEyebrows(baseMaterialCopy, mainTexture, color);
                break;
            case WearableLiterals.Categories.MOUTH:
                bodyShape.SetupMouth(baseMaterialCopy, mainTexture, color);
                break;
        }

        isReady = true;
    }

    public IEnumerator FetchTextures(System.Action OnComplete)
    {
        if (mainTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);

        if (maskTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);

        mainTexture = null;
        maskTexture = null;

        var representation = wearable.GetRepresentation(bodyShapeType);

        string mainTextureName = representation.contents.FirstOrDefault(x => !x.file.ToLower().Contains("_mask.png"))?.hash;
        string maskName = representation.contents.FirstOrDefault(x => x.file.ToLower().Contains("_mask.png"))?.hash;

        if (!string.IsNullOrEmpty(mainTextureName))
        {
            mainTexturePromise = new AssetPromise_Texture(wearable.baseUrl + mainTextureName);
            mainTexturePromise.OnSuccessEvent += (x) => mainTexture = x.texture;
            mainTexturePromise.OnFailEvent += (x) => mainTexture = null;

            AssetPromiseKeeper_Texture.i.Keep(mainTexturePromise);
        }

        if (!string.IsNullOrEmpty(maskName))
        {
            maskTexturePromise = new AssetPromise_Texture(wearable.baseUrl + maskName);
            maskTexturePromise.OnSuccessEvent += (x) => maskTexture = x.texture;
            maskTexturePromise.OnFailEvent += (x) => maskTexture = null;

            AssetPromiseKeeper_Texture.i.Keep(maskTexturePromise);
        }

        yield return mainTexturePromise;
        yield return maskTexturePromise;

        OnComplete?.Invoke();
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
}