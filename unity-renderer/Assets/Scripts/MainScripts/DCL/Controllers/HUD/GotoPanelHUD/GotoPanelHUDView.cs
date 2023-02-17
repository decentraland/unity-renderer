using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.GoToPanel
{
    public class GotoPanelHUDView : MonoBehaviour, IGotoPanelHUDView
    {
        [SerializeField] private Button teleportButton;
        [SerializeField] private TextMeshProUGUI panelText;
        [SerializeField] public GameObject container;
        [SerializeField] internal TextMeshProUGUI sceneTitleText;
        [SerializeField] internal TextMeshProUGUI sceneOwnerText;
        [SerializeField] internal Sprite scenePreviewFailImage;
        [SerializeField] internal RawImageFillParent scenePreviewImage;
        [SerializeField] internal GameObject loadingSpinner;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal ShowHideAnimator contentAnimator;

        private bool isDestroyed;
        private AssetPromise_Texture texturePromise;

        internal ParcelCoordinates targetCoordinates;

        public event Action<ParcelCoordinates> OnTeleportPressed;
        public event Action OnClosePressed;

        private void Start()
        {
            teleportButton.onClick.RemoveAllListeners();
            teleportButton.onClick.AddListener(TeleportTo);
            closeButton.onClick.AddListener(ClosePanel);
            cancelButton.onClick.AddListener(ClosePanel);
            container.SetActive(false);
            contentAnimator.OnWillFinishHide += (animator) => Hide();
        }

        public static GotoPanelHUDView CreateView()
        {
            GotoPanelHUDView view = Instantiate(Resources.Load<GameObject>("GotoPanelHUD")).GetComponent<GotoPanelHUDView>();
            view.name = "_GotoPanelHUD";
            return view;
        }

        private void TeleportTo()
        {
            OnTeleportPressed?.Invoke(targetCoordinates);
            ClosePanel();
        }

        public void SetVisible(bool isVisible)
        {
            container.SetActive(isVisible);
            contentAnimator.Show(!isVisible);
            loadingSpinner.SetActive(isVisible);
            scenePreviewImage.texture = null;

            if (!isVisible)
                AudioScriptableObjects.dialogClose.Play(true);
        }

        public void SetPanelInfo(ParcelCoordinates parcelCoordinates)
        {
            MinimapMetadata.MinimapSceneInfo sceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(parcelCoordinates.x, parcelCoordinates.y);
            if (sceneInfo != null)
            {
                sceneTitleText.text = sceneInfo.name;
                sceneOwnerText.text = sceneInfo.owner;
                SetParcelImage(sceneInfo);
            }
            else
            {
                sceneTitleText.text = "Untitled Scene";
                sceneOwnerText.text = "Unknown";
                DisplayThumbnail(scenePreviewFailImage.texture);
            }
            targetCoordinates = parcelCoordinates;
            panelText.text = parcelCoordinates.ToString();
        }

        private void SetParcelImage(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            DisplayThumbnail(scenePreviewFailImage.texture);
            if (!string.IsNullOrEmpty(sceneInfo.previewImageUrl))
            {
                if (texturePromise != null)
                    AssetPromiseKeeper_Texture.i.Forget(texturePromise);

                texturePromise = new AssetPromise_Texture(sceneInfo.previewImageUrl, storeTexAsNonReadable: false);
                texturePromise.OnSuccessEvent += (textureAsset) => { DisplayThumbnail(textureAsset.texture); };
                texturePromise.OnFailEvent += (textureAsset, error) => { DisplayThumbnail(scenePreviewFailImage.texture); };
                AssetPromiseKeeper_Texture.i.Keep(texturePromise);
            }
        }

        private void DisplayThumbnail(Texture2D texture)
        {
            loadingSpinner.SetActive(false);
            scenePreviewImage.texture = texture;
        }

        private void ClosePanel()
        {
            OnClosePressed?.Invoke();
            contentAnimator.Hide(true);
        }

        private void Hide() => container.SetActive(false);

        private void OnDestroy() { isDestroyed = true; }

        public void Dispose()
        {
            if (isDestroyed)
                return;
            isDestroyed = true;
            ClearPromise();
            Destroy(gameObject);
        }

        private void ClearPromise()
        {
            if (texturePromise != null)
            {
                texturePromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                texturePromise = null;
            }
        }

    }
}
