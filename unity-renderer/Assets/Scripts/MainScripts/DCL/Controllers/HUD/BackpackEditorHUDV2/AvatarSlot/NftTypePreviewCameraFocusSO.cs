using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NftTypePreviewCameraFocus", menuName = "Variables/NftTypePreviewCameraFocus")]
public class NftTypePreviewCameraFocusSO : ScriptableObject
{
    [Serializable]
    public class NftTypePreviewCameraFocus
    {
        public string nftType;
        public CharacterPreviewController.CameraFocus cameraFocus;
        public float orthographicZoom;
    }

    [SerializeField] public NftTypePreviewCameraFocus[] previewCameraFocusByNftType;

    public (CharacterPreviewController.CameraFocus cameraFocus, float? orthographicSize) GetPreviewCameraFocus(string category)
    {
        foreach (var nftType in previewCameraFocusByNftType)
        {
            if(nftType.nftType == category)
                return (nftType.cameraFocus, nftType.orthographicZoom);
        }
        return (CharacterPreviewController.CameraFocus.DefaultEditing, null);
    }
}
