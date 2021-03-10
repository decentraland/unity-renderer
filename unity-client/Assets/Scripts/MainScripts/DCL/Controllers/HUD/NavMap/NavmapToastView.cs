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
        private static readonly int triggerLoadingComplete = Animator.StringToHash("LoadingComplete");

        [SerializeField] internal TextMeshProUGUI sceneTitleText;
        [SerializeField] internal TextMeshProUGUI sceneOwnerText;
        [SerializeField] internal TextMeshProUGUI sceneLocationText;
        [SerializeField] internal RectTransform toastContainer;
        [SerializeField] internal GameObject scenePreviewContainer;
        [SerializeField] internal RawImageFillParent scenePreviewImage;
        [SerializeField] internal Sprite scenePreviewFailImage;
        [SerializeField] internal Animator toastAnimator;

        [SerializeField] internal Button goToButton;
        Vector2Int location;
        RectTransform rectTransform;
        MinimapMetadata minimapMetadata;

        AssetPromise_Texture texturePromise = null;

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

            minimapMetadata.OnSceneInfoUpdated += OnMapMetadataInfoUpdated;
        }

        private void OnDestroy()
        {
            minimapMetadata.OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;
            if (texturePromise != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                texturePromise = null;
            }
        }

        public void Populate(Vector2Int coordinates, MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!gameObject.activeSelf)
                AudioScriptableObjects.dialogOpen.Play(true);

            bool sceneInfoExists = sceneInfo != null;

            gameObject.SetActive(true);
            scenePreviewImage.gameObject.SetActive(false);
            location = coordinates;

            PositionToast(coordinates);

            sceneLocationText.text = $"{coordinates.x}, {coordinates.y}";

            sceneOwnerText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.owner));
            sceneTitleText.text = "Untitled Scene";

            bool useDefaultThumbnail =
                !sceneInfoExists || (sceneInfoExists && string.IsNullOrEmpty(sceneInfo.previewImageUrl));

            if (useDefaultThumbnail)
            {
                DisplayThumbnail(scenePreviewFailImage.texture);
                currentImageUrl = "";
            }

            if (sceneInfoExists)
            {
                sceneTitleText.text = sceneInfo.name;
                sceneOwnerText.text = $"Created by: {sceneInfo.owner}";

                if (currentImageUrl == sceneInfo.previewImageUrl)
                {
                    DisplayThumbnail(texturePromise.asset.texture);
                    return;
                }

                if (texturePromise != null)
                {
                    AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                    texturePromise = null;
                }


                if (!string.IsNullOrEmpty(sceneInfo.previewImageUrl))
                {
                    texturePromise = new AssetPromise_Texture(sceneInfo.previewImageUrl, storeTexAsNonReadable: false);
                    texturePromise.OnSuccessEvent += (textureAsset) => { DisplayThumbnail(textureAsset.texture); };
                    texturePromise.OnFailEvent += (textureAsset) => { DisplayThumbnail(scenePreviewFailImage.texture); };
                    AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                }

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

            gameObject.SetActive(false);
        }

        private void OnGotoClick()
        {
            OnGotoClicked?.Invoke();

            WebInterface.GoTo(location.x, location.y);

            OnCloseClick();
        }

        string currentImageUrl;

        private void DisplayThumbnail(Texture2D texture)
        {
            scenePreviewImage.texture = texture;
            toastAnimator.SetTrigger(triggerLoadingComplete);
        }
    }
}
