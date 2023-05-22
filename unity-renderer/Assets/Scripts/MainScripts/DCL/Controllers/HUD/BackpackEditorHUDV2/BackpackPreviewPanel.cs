using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class BackpackPreviewPanel : BaseComponentView
    {
        private const string RESET_PREVIEW_ANIMATION = "Idle";
        private const float CAMERA_MIN_X_LIMIT = 0f;
        private const float CAMERA_MAX_X_LIMIT = 0f;
        private const float CAMERA_MIN_Y_LIMIT = 0f;
        private const float CAMERA_MAX_Y_LIMIT = 1.7f;
        private const float CAMERA_MIN_Z_LIMIT = 1.3f;
        private const float CAMERA_MAX_Z_LIMIT = 3.5f;
        private const float CAMERA_ZOOM_CENTER = 1.4f;
        private const float CAMERA_ZOOM_BOTTOM_MAX_OFFSET = 1.1f;
        private const float CAMERA_ZOOM_TOP_MAX_OFFSET = 0.3f;

        [SerializeField] private RectTransform avatarPreviewPanel;
        [SerializeField] private PreviewCameraRotation avatarPreviewRotation;
        [SerializeField] private PreviewCameraPanning avatarPreviewPanning;
        [SerializeField] private PreviewCameraZoom avatarPreviewZoom;
        [SerializeField] private RawImage avatarPreviewImage;
        [SerializeField] internal GameObject avatarPreviewLoadingSpinner;

        public delegate void OnSnapshotsReady(Texture2D face256, Texture2D body);

        private ICharacterPreviewController characterPreviewController;
        private float prevRenderScale = 1.0f;

        public void Initialize(ICharacterPreviewFactory characterPreviewFactory)
        {
            characterPreviewController = characterPreviewFactory.Create(
                loadingMode: CharacterPreviewMode.WithoutHologram,
                renderTexture: (RenderTexture) avatarPreviewImage.texture,
                isVisible: false,
                cameraFocus: CharacterPreviewController.CameraFocus.DefaultEditing,
                isAvatarShadowActive: true);
            characterPreviewController.SetCameraLimits(
                CAMERA_MIN_X_LIMIT, CAMERA_MAX_X_LIMIT,
                CAMERA_MIN_Y_LIMIT, CAMERA_MAX_Y_LIMIT,
                CAMERA_MIN_Z_LIMIT, CAMERA_MAX_Z_LIMIT);
            characterPreviewController.ConfigureZoom(
                CAMERA_ZOOM_CENTER, CAMERA_ZOOM_BOTTOM_MAX_OFFSET, CAMERA_ZOOM_TOP_MAX_OFFSET);
            characterPreviewController.SetFocus(CharacterPreviewController.CameraFocus.DefaultEditing);
            avatarPreviewRotation.OnHorizontalRotation += OnPreviewRotation;
            avatarPreviewPanning.OnPanning += OnPreviewPanning;
            avatarPreviewZoom.OnZoom += OnPreviewZoom;
        }

        public override void Dispose()
        {
            base.Dispose();

            characterPreviewController.Dispose();
            avatarPreviewRotation.OnHorizontalRotation -= OnPreviewRotation;
            avatarPreviewPanning.OnPanning -= OnPreviewPanning;
            avatarPreviewZoom.OnZoom -= OnPreviewZoom;
        }

        public override void RefreshControl() { }

        public void SetPreviewEnabled(bool isEnabled)
        {
            characterPreviewController.SetEnabled(isEnabled);
            FixThePreviewRenderingSomehowRelatedToTheRenderScale(isEnabled);
        }

        public void PlayPreviewEmote(string emoteId) =>
            characterPreviewController.PlayEmote(emoteId, (long)Time.realtimeSinceStartup);

        public void PlayPreviewEmote(string emoteId, long timestamp) =>
            characterPreviewController.PlayEmote(emoteId, timestamp);

        public void ResetPreviewEmote() =>
            PlayPreviewEmote(RESET_PREVIEW_ANIMATION);

        public async UniTask TryUpdatePreviewModelAsync(AvatarModel avatarModelToUpdate, CancellationToken ct) =>
            await characterPreviewController.TryUpdateModelAsync(avatarModelToUpdate, ct);

        public void AnchorPreviewPanel(bool anchorRight)
        {
            avatarPreviewPanel.pivot = new Vector2(anchorRight ? 1 : 0, avatarPreviewPanel.pivot.y);
            avatarPreviewPanel.anchorMin = new Vector2(anchorRight ? 1 : 0, avatarPreviewPanel.anchorMin.y);
            avatarPreviewPanel.anchorMax = new Vector2(anchorRight ? 1 : 0, avatarPreviewPanel.anchorMax.y);
            avatarPreviewPanel.offsetMin = new Vector2(anchorRight ? -avatarPreviewPanel.rect.width : 0, avatarPreviewPanel.offsetMin.y);
            avatarPreviewPanel.offsetMax = new Vector2(anchorRight ? 0 : avatarPreviewPanel.rect.width, avatarPreviewPanel.offsetMax.y);
        }

        public void SetLoadingActive(bool isActive) =>
            avatarPreviewLoadingSpinner.SetActive(isActive);

        public void TakeSnapshots(OnSnapshotsReady onSuccess, Action onFailed) =>
            characterPreviewController.TakeSnapshots(
                (face256, body) => onSuccess?.Invoke(face256, body),
                () => onFailed?.Invoke());

        public void SetFocus(CharacterPreviewController.CameraFocus focus, bool useTransition = true) =>
            characterPreviewController.SetFocus(focus, useTransition);

        private void OnPreviewRotation(float angularVelocity) =>
            characterPreviewController.Rotate(angularVelocity);

        private void OnPreviewPanning(Vector3 positionDelta) =>
            characterPreviewController.MoveCamera(positionDelta, true);

        private void OnPreviewZoom(Vector3 delta) =>
            characterPreviewController.MoveCamera(delta, true);

        // TODO: We have to investigate why we have to use this workaround to fix the preview rendering
        private void FixThePreviewRenderingSomehowRelatedToTheRenderScale(bool isEnabled)
        {
            // NOTE(Brian): SSAO doesn't work correctly with the offset avatar preview if the renderScale != 1.0
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (asset == null)
                return;

            if (isEnabled)
            {
                prevRenderScale = asset.renderScale;
                asset.renderScale = 1.0f;
            }
            else
                asset.renderScale = prevRenderScale;
        }
    }
}
