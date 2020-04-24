using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoCollectibleItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;

    internal WearableItem collectible;

    public void Initialize(WearableItem collectible)
    {
        if (collectible != null)
            ForgetThumbnail(collectible.ComposeThumbnailUrl());

        this.collectible = collectible;
        if (this.collectible == null) return;

        if (gameObject.activeInHierarchy)
            GetThumbnail(this.collectible.ComposeThumbnailUrl());
    }

    private void OnEnable()
    {
        if (collectible == null) return;

        var url = collectible.ComposeThumbnailUrl();

        if (string.IsNullOrEmpty(url)) return;

        GetThumbnail(collectible.ComposeThumbnailUrl());
    }

    private void OnDisable()
    {
        if (collectible != null)
            ForgetThumbnail(collectible.ComposeThumbnailUrl());
    }

    private void GetThumbnail(string url)
    {
        ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
    }

    private void ForgetThumbnail(string url)
    {
        ThumbnailsManager.ForgetThumbnail(url, OnThumbnailReady);
    }

    private void OnThumbnailReady(Sprite sprite)
    {
        thumbnail.sprite = sprite;
    }
}