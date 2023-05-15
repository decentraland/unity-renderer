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
        void SetCameraLimits(float? minX, float? maxX, float? minY, float? maxY, float? minZ, float? maxZ, float? minOrthographicSize, float? maxOrthographicSize);
        void SetCameraProjection(bool isOrthographic);
        void SetCameraOrthographicSize(float size);
    }
}
