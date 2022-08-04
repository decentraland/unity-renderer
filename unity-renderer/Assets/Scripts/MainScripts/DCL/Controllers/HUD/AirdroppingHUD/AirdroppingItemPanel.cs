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

    private void OnEnable() { GetThumbnail(); }

    public void ThumbnailReady(Asset_Texture texture)
    {
        thumbnail.sprite = ThumbnailsManager.GetOrCreateSpriteFromTexture(texture.texture, out _);
    }

    private void GetThumbnail()
    {
        ThumbnailsManager.GetThumbnail(currentThumbnailUrl, ThumbnailReady);
    }
}