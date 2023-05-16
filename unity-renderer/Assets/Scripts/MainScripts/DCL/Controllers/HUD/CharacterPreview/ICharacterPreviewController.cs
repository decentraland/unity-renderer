using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface ICharacterPreviewController : IDisposable
    {
        void PlayEmote(string emoteId, long timestamp);
        UniTask TryUpdateModelAsync(AvatarModel newModel, CancellationToken cancellationToken = default);
        void SetFocus(CharacterPreviewController.CameraFocus focus, float? orthographicSize = null, bool useTransition = true);
        void SetEnabled(bool enabled);
        void TakeSnapshots(CharacterPreviewController.OnSnapshotsReady onSuccess, Action onFailed);
        void Rotate(float rotationVelocity);
        void ResetRotation();
        void MoveCamera(Vector3 positionDelta);
        void SetCameraProjection(bool isOrthographic);
        void SetOrthographicLimits(float minY, float maxY);
        void SetCameraOrthographicSize(float size, float minOrthographicSize, float maxOrthographicSize);
    }
}
