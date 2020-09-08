using System;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

internal class BaseCellView : MonoBehaviour
{
    [SerializeField] RawImageFillParent thumbnailImage;
    [SerializeField] GameObject loadingSpinner;
    [SerializeField] Sprite errorThumbnail;

    public event Action<Texture2D> OnThumbnailSet;

    UnityWebRequest thumbnailRequest = null;
    Texture2D thumbnailTexture;

    public void FetchThumbnail(string url, Action onFetchFail)
    {
        if (thumbnailTexture != null)
        {
            OnThumbnailSet?.Invoke(thumbnailTexture);
        }
        else if (string.IsNullOrEmpty(url))
        {
            onFetchFail?.Invoke();
        }
        else if (thumbnailRequest == null)
        {
            thumbnailImage.texture = null;

            thumbnailRequest = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation op = thumbnailRequest.SendWebRequest();
            op.completed += (_) =>
            {
                if (thumbnailRequest == null)
                    return;

                bool success = thumbnailRequest.WebRequestSucceded();
                if (success)
                {
                    thumbnailTexture = ((DownloadHandlerTexture)thumbnailRequest.downloadHandler).texture;
                    thumbnailTexture.Compress(true);
                    SetThumbnail(thumbnailTexture);
                }

                thumbnailRequest.Dispose();
                thumbnailRequest = null;

                if (!success)
                {
                    Debug.Log($"Error downloading: {url}");
                    onFetchFail?.Invoke();
                }
            };
        }
    }

    public void SetDefaultThumbnail()
    {
        SetThumbnail(errorThumbnail.texture);
    }

    public Texture2D GetThumbnail()
    {
        return thumbnailTexture;
    }

    protected virtual void OnEnable()
    {
        if (thumbnailTexture == null)
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

    private void SetThumbnail(Texture2D textureToSet)
    {
        thumbnailTexture = textureToSet;
        thumbnailImage.texture = textureToSet;
        loadingSpinner.SetActive(false);
        OnThumbnailSet?.Invoke(textureToSet);

        if (HUDAudioPlayer.i != null)
            HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.listItemAppear);
    }
}
