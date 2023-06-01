using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraController : IPreviewCameraController
    {
        private const int SUPER_SAMPLING = 1;
        private const float CAMERA_TRANSITION_TIME = 0.3f;

        private Camera camera;
        private Transform cameraTransform;
        private Bounds cameraLimits;
        private (float verticalCenterRef, float bottomMaxOffset, float topMaxOffset) zoomConfig;
        private CancellationTokenSource cts = new ();

        public RenderTexture CurrentTargetTexture => camera.targetTexture;

        public void SetCamera(Camera cameraToUse, RenderTexture targetTexture = null)
        {
            this.camera = cameraToUse;
            cameraTransform = this.camera.transform;
            SetTargetTexture(targetTexture);
        }

        public void SetTargetTexture(RenderTexture targetTexture) =>
            camera.targetTexture = targetTexture;

        public void SetCameraEnabled(bool isEnabled) =>
            camera.enabled = isEnabled;

        public void SetCameraLimits(Bounds limits) =>
            cameraLimits = limits;

        public void ConfigureZoom(float verticalCenterRef, float bottomMaxOffset, float topMaxOffset)
        {
            zoomConfig.verticalCenterRef = verticalCenterRef;
            zoomConfig.bottomMaxOffset = bottomMaxOffset;
            zoomConfig.topMaxOffset = topMaxOffset;
        }

        public Texture2D TakeSnapshot(int width, int height)
        {
            RenderTexture rt = new RenderTexture(width * SUPER_SAMPLING, height * SUPER_SAMPLING, 32);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenShot.Apply();

            return screenShot;
        }

        public void SetFocus(Transform transformToMove, bool useTransition = true)
        {
            cts = cts.SafeRestart();

            if (useTransition)
            {
                CameraTransitionAsync(cameraTransform.position, transformToMove.position,
                    cameraTransform.rotation, transformToMove.rotation,
                    CAMERA_TRANSITION_TIME,
                    cts.Token).Forget();
            }
            else
            {
                cameraTransform.position = transformToMove.position;
                cameraTransform.rotation = transformToMove.rotation;
            }
        }

        public void MoveCamera(Vector3 positionDelta, bool changeYLimitsDependingOnZPosition)
        {
            cameraTransform.Translate(positionDelta, Space.World);
            ApplyCameraLimits(changeYLimitsDependingOnZPosition);
        }

        public void Dispose() =>
            cts.SafeCancelAndDispose();

        private async UniTask CameraTransitionAsync(
            Vector3 initPos, Vector3 endPos,
            Quaternion initRotation, Quaternion endRotation,
            float time, CancellationToken ct)
        {
            float currentTime = 0;
            float inverseTime = 1 / time;

            while (currentTime < time)
            {
                currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0, time);
                cameraTransform.position = Vector3.Lerp(initPos, endPos, currentTime * inverseTime);
                cameraTransform.rotation = Quaternion.Lerp(initRotation, endRotation, currentTime * inverseTime);
                await UniTask.NextFrame(ct);
            }
        }

        private void ApplyCameraLimits(bool changeYLimitsDependingOnZPosition)
        {
            Vector3 pos = cameraTransform.localPosition;
            pos.x = Mathf.Clamp(pos.x, cameraLimits.min.x, cameraLimits.max.x);
            pos.z = Mathf.Clamp(pos.z, cameraLimits.min.z, cameraLimits.max.z);

            pos.y = changeYLimitsDependingOnZPosition ?
                GetCameraYPositionBasedOnZPosition() :
                Mathf.Clamp(pos.y, cameraLimits.min.y, cameraLimits.max.y);

            cameraTransform.localPosition = pos;
        }

        private float GetCameraYPositionBasedOnZPosition()
        {
            Vector3 cameraPosition = cameraTransform.localPosition;
            float minY = zoomConfig.verticalCenterRef - GetOffsetBasedOnZLimits(cameraPosition.z, zoomConfig.bottomMaxOffset);
            float maxY = zoomConfig.verticalCenterRef + GetOffsetBasedOnZLimits(cameraPosition.z, zoomConfig.topMaxOffset);
            return Mathf.Clamp(cameraPosition.y, minY, maxY);
        }

        private float GetOffsetBasedOnZLimits(float zValue, float maxOffset)
        {
            if (zValue >= cameraLimits.max.z) return 0f;
            if (zValue <= cameraLimits.min.z) return maxOffset;
            float progress = (zValue - cameraLimits.min.z) / (cameraLimits.max.z - cameraLimits.min.z);
            return maxOffset - (progress * maxOffset);
        }
    }
}
