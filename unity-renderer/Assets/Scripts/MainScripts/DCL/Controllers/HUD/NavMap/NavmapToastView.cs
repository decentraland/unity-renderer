using DCL.Interface;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapToastView : MonoBehaviour
    {
        [SerializeField] internal TextMeshProUGUI sceneTitleText;
        [SerializeField] internal TextMeshProUGUI sceneOwnerText;
        [SerializeField] internal TextMeshProUGUI sceneLocationText;
        [SerializeField] internal TextMeshProUGUI sceneDescriptionText;
        [SerializeField] internal RectTransform toastContainer;
        [SerializeField] internal GameObject scenePreviewContainer;
        [SerializeField] internal RawImageFillParent scenePreviewImage;
        [SerializeField] internal GameObject scenePreviewLoadingSpinner;
        [SerializeField] internal Sprite scenePreviewFailImage;

        [SerializeField] internal Button goToButton;
        [SerializeField] internal Button closeButton;
        Vector2Int location;
        RectTransform rectTransform;
        MinimapMetadata minimapMetadata;

        public System.Action OnGotoClicked;

        public bool isOpen
        {
            get { return gameObject.activeInHierarchy; }
        }

        private void Awake()
        {
            minimapMetadata = MinimapMetadata.GetMetadata();
            rectTransform = transform as RectTransform;

            goToButton.onClick.AddListener(OnGotoClick);
            closeButton.onClick.AddListener(OnCloseClick);

            minimapMetadata.OnSceneInfoUpdated += OnMapMetadataInfoUpdated;
        }

        private void OnDestroy()
        {
            minimapMetadata.OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;
        }

        public void Populate(Vector2Int coordinates, MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!gameObject.activeSelf)
                AudioScriptableObjects.dialogOpen.Play(true);

            bool sceneInfoExists = sceneInfo != null;

            MapRenderer.i.showCursorCoords = false;

            gameObject.SetActive(true);
            location = coordinates;

            PositionToast(coordinates);

            sceneLocationText.text = $"{coordinates.x}, {coordinates.y}";

            sceneOwnerText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.owner));
            sceneDescriptionText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.description));
            sceneTitleText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.name));
            scenePreviewLoadingSpinner.SetActive(false);

            bool useDefaultThumbnail =
                !sceneInfoExists || (sceneInfoExists && string.IsNullOrEmpty(sceneInfo.previewImageUrl));

            if (useDefaultThumbnail)
            {
                scenePreviewImage.texture = scenePreviewFailImage.texture;
                currentImageUrl = "";
            }

            if (sceneInfoExists)
            {
                sceneTitleText.text = sceneInfo.name;
                sceneOwnerText.text = $"Created by: {sceneInfo.owner}";
                sceneDescriptionText.text = sceneInfo.description;

                if (currentImageUrl == sceneInfo.previewImageUrl) return;

                if (currentImage != null)
                    Destroy(currentImage);

                if (downloadCoroutine != null)
                    CoroutineStarter.Stop(downloadCoroutine);

                if (sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.previewImageUrl))
                    downloadCoroutine = CoroutineStarter.Start(Download(sceneInfo.previewImageUrl));

                currentImageUrl = sceneInfoExists ? sceneInfo.previewImageUrl : "";
            }
        }

        public void OnMapMetadataInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!isOpen) return;

            bool updatedCurrentLocationInfo = false;
            foreach (Vector2Int parcel in sceneInfo.parcels)
            {
                if (parcel == location)
                {
                    updatedCurrentLocationInfo = true;
                    break;
                }
            }

            if (updatedCurrentLocationInfo)
                Populate(location, sceneInfo);
        }

        void PositionToast(Vector2Int coordinates)
        {
            if (toastContainer == null || rectTransform == null) return;

            // position the toast over the parcel parcelHighlightImage so that we can easily check with LOCAL pos info where it is on the screen
            toastContainer.position = MapRenderer.i.parcelHighlightImage.transform.position;

            bool useBottom = toastContainer.localPosition.y > 0;

            bool shouldOffsetHorizontally = Mathf.Abs(toastContainer.localPosition.x) > rectTransform.rect.width / 4;
            bool useLeft = false;

            if (shouldOffsetHorizontally)
                useLeft = toastContainer.localPosition.x > 0;

            // By setting the pivot accordingly BEFORE we position the toast, we can have it always visible in an easier way
            toastContainer.pivot = new Vector2(shouldOffsetHorizontally ? (useLeft ? 1 : 0) : 0.5f, useBottom ? 1 : 0);
            toastContainer.position = MapRenderer.i.parcelHighlightImage.transform.position;

        }

        public void OnCloseClick()
        {
            if (gameObject.activeSelf)
                AudioScriptableObjects.dialogClose.Play(true);

            MapRenderer.i.showCursorCoords = true;
            gameObject.SetActive(false);
        }

        private void OnGotoClick()
        {
            OnGotoClicked?.Invoke();

            WebInterface.GoTo(location.x, location.y);

            OnCloseClick();
        }

        string currentImageUrl;
        Texture2D currentImage;
        Coroutine downloadCoroutine;

        private IEnumerator Download(string url)
        {
            scenePreviewLoadingSpinner.SetActive(true);
            scenePreviewImage.texture = null;

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                currentImage = ((DownloadHandlerTexture)www.downloadHandler).texture;
                currentImage.Compress(false);
                scenePreviewImage.texture = currentImage;
            }
            else
            {
                Debug.Log($"Error downloading: {url} {www.error}");
                scenePreviewImage.texture = scenePreviewFailImage.texture;
            }
            scenePreviewLoadingSpinner.SetActive(false);
        }
    }
}
