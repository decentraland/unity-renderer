using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AirdroppingItemPanel : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI name;
    [SerializeField] internal TextMeshProUGUI subtitle;
    [SerializeField] internal Image thumbnail;

    private string currentThumbnail;

    public void SetData(string name, string subtitle, string thumbnailURL)
    {
        if (currentThumbnail != null)
            ThumbnailsManager.CancelRequest(currentThumbnail, ThumbnailReady);

        this.name.text = name;
        this.name.gameObject.SetActive(!string.IsNullOrEmpty(this.name.text));

        this.subtitle.text = subtitle;
        this.subtitle.gameObject.SetActive(!string.IsNullOrEmpty(this.subtitle.text));

        currentThumbnail = thumbnailURL;
        ThumbnailsManager.RequestThumbnail(currentThumbnail, ThumbnailReady);
    }

    public void ThumbnailReady(Sprite sprite)
    {
        thumbnail.sprite = sprite;
    }

    private void OnDestroy()
    {
        if (currentThumbnail != null)
            ThumbnailsManager.CancelRequest(currentThumbnail, ThumbnailReady);
    }
}