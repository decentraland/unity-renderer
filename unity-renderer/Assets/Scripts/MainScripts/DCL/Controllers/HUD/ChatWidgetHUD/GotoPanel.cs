using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DCL.Interface;
using DCL;

public class GotoPanel : MonoBehaviour
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

    private ParcelCoordinates targetCoordinates;

    AssetPromise_Texture texturePromise = null;

    private void Start()
    {
        teleportButton.onClick.RemoveAllListeners();
        teleportButton.onClick.AddListener(TeleportTo);
        closeButton.onClick.AddListener(OnClosePressed);
        cancelButton.onClick.AddListener(OnClosePressed);
        container.SetActive(false);
        contentAnimator.OnWillFinishHide += (animator) => Hide();
    }

    private void TeleportTo()
    {
        WebInterface.GoTo(targetCoordinates.x, targetCoordinates.y);
        contentAnimator.Hide(true);
    }

    public void SetPanelInfo(ParcelCoordinates parcelCoordinates)
    {
        container.SetActive(true);
        contentAnimator.Show(false);
        loadingSpinner.SetActive(true);
        scenePreviewImage.texture = null;
        MinimapMetadata.MinimapSceneInfo sceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(parcelCoordinates.x, parcelCoordinates.y);
        if (sceneInfo != null)
        {
            sceneTitleText.text = sceneInfo.name;
            sceneOwnerText.text = sceneInfo.owner;
            SetParcelImage(sceneInfo);
        } else 
        {
            sceneTitleText.text = "Untitled Scene";
            sceneOwnerText.text = "Unknown"; 
            DisplayThumbnail(scenePreviewFailImage.texture);
        }
        targetCoordinates = parcelCoordinates;
        panelText.text = $"{parcelCoordinates.x},{parcelCoordinates.y}";
    }

    private void SetParcelImage(MinimapMetadata.MinimapSceneInfo sceneInfo)
    {
        if (!string.IsNullOrEmpty(sceneInfo.previewImageUrl))
        {
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

    private void OnClosePressed()
    {
        contentAnimator.Hide(true);
        AudioScriptableObjects.dialogClose.Play(true);
    }

    private void Hide() => container.SetActive(false);

}
