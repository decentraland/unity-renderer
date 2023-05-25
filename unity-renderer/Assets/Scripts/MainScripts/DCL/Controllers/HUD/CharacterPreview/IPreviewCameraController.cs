using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface IPreviewCameraController
    {
        RenderTexture CurrentTargetTexture { get; }

        void SetCamera(Camera camera, RenderTexture targetTexture = null);
        void SetTargetTexture(RenderTexture targetTexture);
        void SetCameraEnabled(bool isEnabled);
        void SetCameraLimits(Bounds limits);
        void ConfigureZoom(float verticalCenterRef, float bottomMaxOffset, float topMaxOffset);
        Texture2D TakeSnapshot(int width, int height);
        void SetFocus(Transform transformToMove, bool useTransition = true);
        void MoveCamera(Vector3 positionDelta, bool changeYLimitsDependingOnZPosition);
        void Dispose();
    }
}
