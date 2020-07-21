using DCL;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoCollectibleItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;

    internal WearableItem collectible;
    private AssetPromise_Texture thumbnailPromise;

    public void Initialize(WearableItem collectible)
    {
        this.collectible = collectible;

        if (this.collectible == null) return;

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    private void OnEnable()
    {
        if (collectible == null) return;

        GetThumbnail();
    }

    private void OnDisable()
    {
        if (collectible != null)
            ForgetThumbnail();
    }

    private void GetThumbnail()
    {
        string url = collectible.ComposeThumbnailUrl();
        //NOTE(Brian): Get before forget to prevent referenceCount == 0 and asset unload
        var newThumbnailPromise = ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
        ForgetThumbnail();
        thumbnailPromise = newThumbnailPromise;
    }

    private void ForgetThumbnail()
    {
        ThumbnailsManager.ForgetThumbnail(thumbnailPromise);
    }

    private void OnThumbnailReady(Asset_Texture texture)
    {
        if (thumbnail.sprite != null)
            Destroy(thumbnail.sprite);

        thumbnail.sprite = ThumbnailsManager.CreateSpriteFromTexture(texture.texture);
    }
}