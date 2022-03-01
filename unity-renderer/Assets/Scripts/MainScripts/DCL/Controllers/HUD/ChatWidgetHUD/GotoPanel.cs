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

    private ParcelCoordinates targetCoordinates;
    MinimapMetadata mapMetadata;

    AssetPromise_Texture texturePromise = null;

    private void Start()
    {
        teleportButton.onClick.RemoveAllListeners();
        teleportButton.onClick.AddListener(TeleportTo);
        mapMetadata = MinimapMetadata.GetMetadata();
    }

    private void TeleportTo()
    {
        WebInterface.GoTo(targetCoordinates.x, targetCoordinates.y);
        container.SetActive(false);
    }

    public void SetPanelInfo(ParcelCoordinates parcelCoordinates)
    {
        MinimapMetadata.MinimapSceneInfo sceneInfo = mapMetadata.GetSceneInfo(parcelCoordinates.x, parcelCoordinates.y);
        if (sceneInfo != null)
        {
            SetParcelImage(sceneInfo);
            sceneTitleText.text = sceneInfo.name;
            sceneOwnerText.text = sceneInfo.owner;
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
        scenePreviewImage.texture = texture;
    }

}
