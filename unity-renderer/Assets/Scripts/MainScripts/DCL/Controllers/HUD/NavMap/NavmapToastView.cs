using DCL.Interface;
using TMPro;
using UnityEngine;
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

        AssetPromise_Texture texturePromise;
        string currentImageUrl;

        private void Open(int cursorTileX, int cursorTileY)
        {
            if (gameObject.activeInHierarchy)
                Close();
            
            var sceneInfo = minimapMetadata.GetSceneInfo(cursorTileX, cursorTileY);
            if (sceneInfo == null)
                WebInterface.RequestScenesInfoAroundParcel(new Vector2(cursorTileX, cursorTileY), 15);

            Populate(new Vector2Int(cursorTileX, cursorTileY), sceneInfo);
        }

        private void Awake()
        {
            minimapMetadata = MinimapMetadata.GetMetadata();
            rectTransform = transform as RectTransform;
        }

        private void Start()
        {
            gameObject.SetActive(false);
            
            goToButton.onClick.AddListener(OnGotoClick);
            
            minimapMetadata.OnSceneInfoUpdated += OnMapMetadataInfoUpdated;
            MapRenderer.OnParcelClicked += Open;
            MapRenderer.OnCursorFarFromParcel += Close;
        }

        private void OnDestroy()
        {
            minimapMetadata.OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;
            MapRenderer.OnParcelClicked -= Open;
            MapRenderer.OnCursorFarFromParcel -= Close;

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
                    DisplayThumbnail(texturePromise?.asset?.texture);
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
                    texturePromise.OnFailEvent += (textureAsset, error) => { DisplayThumbnail(scenePreviewFailImage.texture); };
                    AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                }

                currentImageUrl = sceneInfoExists ? sceneInfo.previewImageUrl : "";
            }
        }

        public void OnMapMetadataInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!gameObject.activeInHierarchy)
                return;

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
            if (toastContainer == null || rectTransform == null)
                return;

            // position the toast over the parcel parcelHighlightImage so that we can easily check with LOCAL pos info where it is on the screen
            toastContainer.position = MapRenderer.i.GetParcelHighlightTransform();

            bool useBottom = toastContainer.localPosition.y > 0;

            bool shouldOffsetHorizontally = Mathf.Abs(toastContainer.localPosition.x) > rectTransform.rect.width / 4;
            bool useLeft = false;

            if (shouldOffsetHorizontally)
                useLeft = toastContainer.localPosition.x > 0;

            // By setting the pivot accordingly BEFORE we position the toast, we can have it always visible in an easier way
            toastContainer.pivot = new Vector2(shouldOffsetHorizontally ? (useLeft ? 1 : 0) : 0.5f, useBottom ? 1 : 0);
            toastContainer.position = MapRenderer.i.GetParcelHighlightTransform();

        }

        public void Close()
        {
            if (gameObject.activeSelf)
                AudioScriptableObjects.dialogClose.Play(true);

            gameObject.SetActive(false);
        }

        private void OnGotoClick()
        {
            DataStore.i.HUDs.navmapVisible.Set(false);
            Environment.i.world.teleportController.Teleport(location.x, location.y);

            Close();
        }
        
        private void DisplayThumbnail(Texture2D texture)
        {
            scenePreviewImage.texture = texture;
            toastAnimator.SetTrigger(triggerLoadingComplete);
        }
    }
}