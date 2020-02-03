using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoCollectibleItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;

    internal WearableItem collectible;

    public void Initialize(WearableItem collectible)
    {
        if(collectible != null)
            ThumbnailsManager.CancelRequest(collectible.ComposeThumbnailUrl(), OnThumbnailReady);
        this.collectible = collectible;
        if (this.collectible == null) return;

        ThumbnailsManager.RequestThumbnail(this.collectible.ComposeThumbnailUrl(), OnThumbnailReady);
    }

    private void OnThumbnailReady(Sprite sprite)
    {
        thumbnail.sprite = sprite;
    }

    private void OnDestroy()
    {
        if(collectible != null)
            ThumbnailsManager.CancelRequest(collectible.ComposeThumbnailUrl(), OnThumbnailReady);
    }
}