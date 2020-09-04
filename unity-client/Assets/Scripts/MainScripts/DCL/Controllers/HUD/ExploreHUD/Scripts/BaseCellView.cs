using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

internal class BaseCellView : MonoBehaviour
{
    [SerializeField] Image thumbnailImage;
    [SerializeField] GameObject loadingSpinner;
    [SerializeField] Sprite errorThumbnail;

    public event Action<Sprite> OnThumbnailFetched;

    UnityWebRequest thumbnailRequest = null;
    Texture2D thumbnailTexture;
    Sprite thumbnail;

    public void FetchThumbnail(string url)
    {
        if (thumbnail)
        {
            OnThumbnailFetched?.Invoke(thumbnail);
        }
        else if (thumbnailRequest == null)
        {
            thumbnailImage.sprite = null;

            thumbnailRequest = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation op = thumbnailRequest.SendWebRequest();
            op.completed += (_) =>
            {
                if (thumbnailRequest == null)
                    return;

                if (thumbnailRequest.WebRequestSucceded())
                {
                    thumbnailTexture = ((DownloadHandlerTexture)thumbnailRequest.downloadHandler).texture;
                    thumbnailTexture.Compress(false);
                    thumbnail = Sprite.Create(thumbnailTexture, new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), Vector2.zero);
                }
                else
                {
                    Debug.Log($"Error downloading: {url} {thumbnailRequest.error}");
                    thumbnail = errorThumbnail;
                }

                thumbnailImage.sprite = thumbnail;
                loadingSpinner.SetActive(false);

                thumbnailRequest.Dispose();
                thumbnailRequest = null;

                if (HUDAudioPlayer.i != null)
                    HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.listItemAppear);

                OnThumbnailFetched?.Invoke(thumbnail);
            };
        }
    }

    public Sprite GetThumbnail()
    {
        return thumbnail;
    }

    protected virtual void OnEnable()
    {
        if (thumbnail == null)
        {
            loadingSpinner.SetActive(true);
        }
        else
        {
            loadingSpinner.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        GameObject.Destroy(thumbnailTexture);
        if (thumbnailRequest != null)
        {
            thumbnailRequest.Abort();
            thumbnailRequest.Dispose();
            thumbnailRequest = null;
        }
    }
}
