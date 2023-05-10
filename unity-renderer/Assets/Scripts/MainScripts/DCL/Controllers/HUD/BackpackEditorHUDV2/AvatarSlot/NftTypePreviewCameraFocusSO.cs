using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UnityEngine;

[CreateAssetMenu(fileName = "NftTypePreviewCameraFocus", menuName = "Variables/NftTypePreviewCameraFocus")]
public class NftTypePreviewCameraFocusSO : ScriptableObject
{
    [SerializeField] public SerializableKeyValuePair<string, CharacterPreviewController.CameraFocus>[] previewCameraFocusByNftType;

    public CharacterPreviewController.CameraFocus GetPreviewCameraFocus(string category)
    {
        foreach (var nftType in previewCameraFocusByNftType)
        {
            if(nftType.key == category)
                return nftType.value;
        }
        return CharacterPreviewController.CameraFocus.DefaultEditing;
    }
}
