using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AirdroppingItemPanel : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI name;
    [SerializeField] internal TextMeshProUGUI subtitle;
    [SerializeField] internal Image thumbnail;

    private string currentThumbnailUrl;
    private AssetPromise_Texture currentThumbnailPromise;

    public void SetData(string name, string subtitle, string thumbnailURL)
    {
        this.name.text = name;
        this.name.gameObject.SetActive(!string.IsNullOrEmpty(this.name.text));

        this.subtitle.text = subtitle;
        this.subtitle.gameObject.SetActive(!string.IsNullOrEmpty(this.subtitle.text));

        currentThumbnailUrl = thumbnailURL;

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    private void OnEnable()
    {
        GetThumbnail();
    }

    private void OnDisable()
    {
        ForgetThumbnail();
    }

    public void ThumbnailReady(Asset_Texture texture)
    {
        if (thumbnail.sprite != null)
            Destroy(thumbnail.sprite);

        thumbnail.sprite = ThumbnailsManager.CreateSpriteFromTexture(texture.texture);
    }

    private void GetThumbnail()
    {
        var newCurrentThumbnailPromise = ThumbnailsManager.GetThumbnail(currentThumbnailUrl, ThumbnailReady);
        ThumbnailsManager.ForgetThumbnail(currentThumbnailPromise);
        currentThumbnailPromise = newCurrentThumbnailPromise;
    }

    private void ForgetThumbnail()
    {
        if (currentThumbnailPromise == null)
            return;

        ThumbnailsManager.ForgetThumbnail(currentThumbnailPromise);
        ThumbnailReady(null);
        currentThumbnailPromise = null;
    }
}