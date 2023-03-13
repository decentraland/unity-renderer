using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DCL.GoToPanel
{
    public class GotoPanelHUDView : MonoBehaviour, IGotoPanelHUDView
    {
        [SerializeField] internal Button teleportButton;
        [SerializeField] internal TextMeshProUGUI panelText;
        [SerializeField] internal GameObject container;
        [SerializeField] internal TextMeshProUGUI sceneTitleText;
        [SerializeField] internal TextMeshProUGUI sceneOwnerText;
        [SerializeField] internal Sprite scenePreviewFailImage;
        [SerializeField] internal RawImageFillParent scenePreviewImage;
        [SerializeField] internal GameObject imageLoadingContainer;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal ShowHideAnimator contentAnimator;
        [SerializeField] internal GameObject sceneMetadataLoadingContainer;

        private bool isDestroyed;
        private AssetPromise_Texture texturePromise;
        private ParcelCoordinates targetCoordinates;

        public event Action<ParcelCoordinates> OnTeleportPressed;
        public event Action OnClosePressed;

        private void Awake()
        {
            teleportButton.onClick.AddListener(TeleportTo);
            closeButton.onClick.AddListener(ClosePanel);
            cancelButton.onClick.AddListener(ClosePanel);
            container.SetActive(false);
            contentAnimator.OnWillFinishHide += (animator) => Hide();
        }

        private void OnDestroy()
        {
            isDestroyed = true;
        }

        public void Dispose()
        {
            if (isDestroyed)
                return;

            isDestroyed = true;
            ClearPromise();
            Destroy(gameObject);
        }

        public static GotoPanelHUDView CreateView()
        {
            GotoPanelHUDView view = Instantiate(Resources.Load<GotoPanelHUDView>("GotoPanelHUD"));
            view.name = "_GotoPanelHUD";
            return view;
        }

        public void SetVisible(bool isVisible)
        {
            container.SetActive(isVisible);
            contentAnimator.Show(!isVisible);
            imageLoadingContainer.SetActive(isVisible);
            scenePreviewImage.texture = null;

            if (!isVisible)
                AudioScriptableObjects.dialogClose.Play(true);
        }

        public void SetPanelInfo(ParcelCoordinates coordinates, MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (sceneInfo != null)
            {
                sceneTitleText.text = sceneInfo.name;
                sceneOwnerText.text = sceneInfo.owner;
                SetParcelImage(sceneInfo.previewImageUrl);
            }
            else
            {
                sceneTitleText.text = "Untitled Scene";
                sceneOwnerText.text = "Unknown";
                DisplayThumbnail(scenePreviewFailImage.texture);
            }

            targetCoordinates = coordinates;
            panelText.text = coordinates.ToString();
        }

        public void ShowLoading() =>
            sceneMetadataLoadingContainer.SetActive(true);

        public void HideLoading() =>
            sceneMetadataLoadingContainer.SetActive(false);

        private void TeleportTo()
        {
            OnTeleportPressed?.Invoke(targetCoordinates);
            ClosePanel();
        }

        private void SetParcelImage(string imageUrl)
        {
            DisplayThumbnail(scenePreviewFailImage.texture);

            if (string.IsNullOrEmpty(imageUrl)) return;

            if (texturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);

            texturePromise = new AssetPromise_Texture(imageUrl, storeTexAsNonReadable: false);
            texturePromise.OnSuccessEvent += (textureAsset) => { DisplayThumbnail(textureAsset.texture); };
            texturePromise.OnFailEvent += (textureAsset, error) => { DisplayThumbnail(scenePreviewFailImage.texture); };
            AssetPromiseKeeper_Texture.i.Keep(texturePromise);
        }

        private void DisplayThumbnail(Texture2D texture)
        {
            imageLoadingContainer.SetActive(false);
            scenePreviewImage.texture = texture;
        }

        private void ClosePanel()
        {
            OnClosePressed?.Invoke();
            contentAnimator.Hide(true);
        }

        private void Hide() =>
            container.SetActive(false);

        private void ClearPromise()
        {
            if (texturePromise == null) return;
            texturePromise.ClearEvents();
            AssetPromiseKeeper_Texture.i.Forget(texturePromise);
            texturePromise = null;
        }
    }
}
