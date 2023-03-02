using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface ICharacterPreviewController : IDisposable
    {
        void PlayEmote(string emoteId, long timestamp);

        UniTask TryUpdateModelAsync(AvatarModel newModel, CancellationToken cancellationToken = default);

        void SetFocus(CharacterPreviewController.CameraFocus focus, bool useTransition = true);

        void SetEnabled(bool enabled);

        void TakeSnapshots(CharacterPreviewController.OnSnapshotsReady onSuccess, Action onFailed);

        void Rotate(float rotationVelocity);

        void ResetRotation();
    }
}
