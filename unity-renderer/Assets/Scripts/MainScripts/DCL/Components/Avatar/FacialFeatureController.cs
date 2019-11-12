using System.Collections;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

public class FacialFeatureController
{
    public delegate void TexturesFetched(Texture texture, Texture mask);

    private string bodyShapeType;
    WearableItem wearable;

    Texture mainTexture = null;
    Texture maskTexture = null;
    bool texturesRetreived = false;
    Coroutine coroutine;

    public FacialFeatureController(WearableItem wearableItem, string bodyShapeType)
    {
        this.wearable = wearableItem;
        this.bodyShapeType = bodyShapeType;
        texturesRetreived = false;
    }

    public IEnumerator FetchTextures(TexturesFetched onTextureFetched)
    {
        if (texturesRetreived)
        {
            onTextureFetched?.Invoke(mainTexture, maskTexture);
            yield break;
        }

        var representation = wearable.GetRepresentation(bodyShapeType);

        string mainTextureUrl = wearable.baseUrl + representation.contents.FirstOrDefault(x => !x.file.ToLower().Contains("_mask.png"))?.hash;
        string maskUrl = wearable.baseUrl + representation.contents.FirstOrDefault(x => x.file.ToLower().Contains("_mask.png"))?.hash;

        if (!string.IsNullOrEmpty(mainTextureUrl))
        {
            yield return Utils.FetchTexture(mainTextureUrl, (tex) =>
            {
                mainTexture = tex;
            });
        }

        if (!string.IsNullOrEmpty(maskUrl))
        {
            yield return Utils.FetchTexture(maskUrl, (tex) =>
            {
                maskTexture = tex;
            });
        }

        texturesRetreived = true;
        onTextureFetched?.Invoke(mainTexture, maskTexture);
    }
}