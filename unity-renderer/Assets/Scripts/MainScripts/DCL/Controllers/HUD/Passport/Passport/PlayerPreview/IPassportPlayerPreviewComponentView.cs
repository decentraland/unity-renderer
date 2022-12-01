using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public interface IPassportPlayerPreviewComponentView
    {
        RawImage CharacterPreviewImage { get; }
        PreviewCameraRotation PreviewCameraRotation { get; }
    }
}
