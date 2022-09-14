using DCL;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoCollectibleItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;

    internal WearableItem collectible;
    private bool finishedLoading = false;

    public void Initialize(WearableItem collectible)
    {
        this.collectible = collectible;

        if (this.collectible == null)
            return;

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    private void OnEnable()
    {
        if (collectible == null)
            return;

        GetThumbnail();
    }
    
    private void GetThumbnail()
    {
        string url = collectible.ComposeThumbnailUrl();
        
        
        ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
    }

    private void OnThumbnailReady(Asset_Texture texture)
    {
        thumbnail.sprite = ThumbnailsManager.GetOrCreateSpriteFromTexture(texture.texture, out _);

        finishedLoading = true;
    }
}