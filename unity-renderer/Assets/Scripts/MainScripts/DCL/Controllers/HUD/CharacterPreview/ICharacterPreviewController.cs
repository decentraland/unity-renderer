using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface ICharacterPreviewController : IDisposable
    {
        IReadOnlyList<SkinnedMeshRenderer> originalVisibleRenderers { get; }

        void PlayEmote(string emoteId, long timestamp);
        UniTask TryUpdateModelAsync(AvatarModel newModel, CancellationToken cancellationToken = default);
        void SetFocus(PreviewCameraFocus focus, bool useTransition = true);
        void SetEnabled(bool enabled);
        UniTask<Texture2D> TakeBodySnapshotAsync();
        void TakeSnapshots(CharacterPreviewController.OnSnapshotsReady onSuccess, Action onFailed);
        void Rotate(float rotationVelocity);
        void ResetRotation();
        void MoveCamera(Vector3 positionDelta, bool changeYLimitsDependingOnZPosition);
        void SetCameraLimits(Bounds limits);
        void ConfigureZoom(float verticalCenterRef, float bottomMaxOffset, float topMaxOffset);
        void SetCharacterShadowActive(bool isActive);
    }
}
