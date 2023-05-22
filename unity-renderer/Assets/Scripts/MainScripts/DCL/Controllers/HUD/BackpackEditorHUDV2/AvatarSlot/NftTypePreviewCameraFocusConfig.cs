using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NftTypePreviewCameraFocus", menuName = "Variables/NftTypePreviewCameraFocus")]
public class NftTypePreviewCameraFocusConfig : ScriptableObject
{
    [Serializable]
    public class NftTypePreviewCameraFocus
    {
        public string nftType;
        public CharacterPreviewController.CameraFocus cameraFocus;
    }

    [SerializeField] public NftTypePreviewCameraFocus[] previewCameraFocusByNftType;

    public CharacterPreviewController.CameraFocus GetPreviewCameraFocus(string category)
    {
        foreach (var nftType in previewCameraFocusByNftType)
        {
            if(nftType.nftType == category)
                return nftType.cameraFocus;
        }
        return CharacterPreviewController.CameraFocus.DefaultEditing;
    }
}
