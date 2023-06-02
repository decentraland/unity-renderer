using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    using UnityEngine;

    public class CharacterPreviewController : MonoBehaviour, ICharacterPreviewController
    {
        private const int SNAPSHOT_BODY_WIDTH_RES = 256;
        private const int SNAPSHOT_BODY_HEIGHT_RES = 512;
        private const int SNAPSHOT_FACE_256_WIDTH_RES = 256;
        private const int SNAPSHOT_FACE_256_HEIGHT_RES = 256;

        public delegate void OnSnapshotsReady(Texture2D face256, Texture2D body);

        private Dictionary<PreviewCameraFocus, Transform> cameraFocusLookUp;

        [SerializeField] private new Camera camera;
        [SerializeField] private Transform defaultEditingTemplate;
        [SerializeField] private Transform faceEditingTemplate;
        [SerializeField] private Transform faceSnapshotTemplate;
        [SerializeField] private Transform bodySnapshotTemplate;
        [SerializeField] private Transform previewTemplate;
        [SerializeField] private GameObject avatarContainer;
        [SerializeField] private Transform baseAvatarContainer;
        [SerializeField] private BaseAvatarReferences baseAvatarReferencesPrefab;
        [SerializeField] private GameObject avatarShadow;

        private Service<IAvatarFactory> avatarFactory;

        private IAvatar avatar;
        private readonly AvatarModel currentAvatarModel = new () { wearables = new List<string>() };
        private CancellationTokenSource loadingCts = new ();
        private IAnimator animator;
        private Quaternion avatarContainerDefaultRotation;
        private Transform cameraTransform;
        private IPreviewCameraController cameraController;

        private void Awake()
        {
            cameraFocusLookUp = new Dictionary<PreviewCameraFocus, Transform>()
            {
                { PreviewCameraFocus.DefaultEditing, defaultEditingTemplate },
                { PreviewCameraFocus.FaceEditing, faceEditingTemplate },
                { PreviewCameraFocus.FaceSnapshot, faceSnapshotTemplate },
                { PreviewCameraFocus.BodySnapshot, bodySnapshotTemplate },
                { PreviewCameraFocus.Preview, previewTemplate }
            };

            this.animator = GetComponentInChildren<IAnimator>();
            avatarContainerDefaultRotation = avatarContainer.transform.rotation;
        }

        public void SetEnabled(bool isEnabled)
        {
            gameObject.SetActive(isEnabled);
            cameraController.SetCameraEnabled(isEnabled);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public void Initialize(
            CharacterPreviewMode loadingMode,
            RenderTexture targetTexture,
            IPreviewCameraController previewCameraController)
        {
            avatar?.Dispose();
            avatar = loadingMode switch
                     {
                         CharacterPreviewMode.WithHologram => CreateAvatarWithHologram(),
                         CharacterPreviewMode.WithoutHologram => CreateAvatar(),
                         _ => avatar,
                     };

            this.cameraController = previewCameraController;
            cameraController.SetCamera(camera, targetTexture);
        }

        public async UniTask TryUpdateModelAsync(AvatarModel newModel, CancellationToken cancellationToken = default)
        {
            if (newModel.HaveSameWearablesAndColors(currentAvatarModel) && avatar.status == IAvatar.Status.Loaded)
                return;

            loadingCts?.Cancel();
            loadingCts?.Dispose();
            loadingCts = new CancellationTokenSource();

            cancellationToken = cancellationToken == default
                ? loadingCts.Token
                : CancellationTokenSource.CreateLinkedTokenSource(loadingCts.Token, cancellationToken).Token;

            await UpdateModelAsync(newModel, cancellationToken);
        }

        private void OnDestroy()
        {
            loadingCts?.Cancel();
            loadingCts?.Dispose();
            loadingCts = null;
            avatar?.Dispose();
            cameraController.Dispose();
        }

        private IAvatar CreateAvatar() =>
            avatarFactory.Ref.CreateAvatar(avatarContainer, this.animator, NoLODs.i, new Visibility());

        private IAvatar CreateAvatarWithHologram()
        {
            var baseAvatarReferences = baseAvatarContainer.GetComponentInChildren<IBaseAvatarReferences>() ?? Instantiate(baseAvatarReferencesPrefab, baseAvatarContainer);

            return avatarFactory.Ref.CreateAvatarWithHologram(avatarContainer, new BaseAvatar(baseAvatarReferences), this.animator, NoLODs.i, new Visibility());
        }

        private async UniTask UpdateModelAsync(AvatarModel newModel, CancellationToken ct)
        {
            currentAvatarModel.CopyFrom(newModel);

            try
            {
                ct.ThrowIfCancellationRequested();
                List<string> wearables = new List<string>(newModel.wearables) { newModel.bodyShape };

                await avatar.Load(wearables, newModel.emotes.Select(x => x.urn).ToList(), new AvatarSettings
                {
                    bodyshapeId = newModel.bodyShape,
                    eyesColor = newModel.eyeColor,
                    hairColor = newModel.hairColor,
                    skinColor = newModel.skinColor,
                    forceRender = new HashSet<string>(newModel.forceRender)
                }, ct);
            }
            catch (Exception e) when (e is not OperationCanceledException) { Debug.LogException(e); }
        }

        public void TakeSnapshots(OnSnapshotsReady onSuccess, Action onFailed)
        {
            if (avatar.status != IAvatar.Status.Loaded)
            {
                onFailed?.Invoke();
                return;
            }

            StartCoroutine(TakeSnapshots_Routine(onSuccess));
        }

        private IEnumerator TakeSnapshots_Routine(OnSnapshotsReady callback)
        {
            global::DCL.Environment.i.platform.cullingController.Stop();

            var current = cameraController.CurrentTargetTexture;
            cameraController.SetTargetTexture(null);
            var avatarAnimator = avatarContainer.gameObject.GetComponentInChildren<AvatarAnimatorLegacy>();

            SetFocus(PreviewCameraFocus.FaceSnapshot, false);
            avatarAnimator.Reset();
            yield return null;
            Texture2D face256 = cameraController.TakeSnapshot(SNAPSHOT_FACE_256_WIDTH_RES, SNAPSHOT_FACE_256_HEIGHT_RES);

            SetFocus(PreviewCameraFocus.BodySnapshot, false);
            avatarAnimator.Reset();
            yield return null;
            Texture2D body = cameraController.TakeSnapshot(SNAPSHOT_BODY_WIDTH_RES, SNAPSHOT_BODY_HEIGHT_RES);

            SetFocus(PreviewCameraFocus.DefaultEditing, false);

            cameraController.SetTargetTexture(current);

            global::DCL.Environment.i.platform.cullingController.Start();
            callback?.Invoke(face256, body);
        }

        public void SetFocus(PreviewCameraFocus focus, bool useTransition = true) =>
            cameraController.SetFocus(cameraFocusLookUp[focus], useTransition);

        public void Rotate(float rotationVelocity) =>
            avatarContainer.transform.Rotate(Time.deltaTime * rotationVelocity * Vector3.up);

        public void ResetRotation() =>
            avatarContainer.transform.rotation = avatarContainerDefaultRotation;

        public void MoveCamera(Vector3 positionDelta, bool changeYLimitsDependingOnZPosition) =>
            cameraController.MoveCamera(positionDelta, changeYLimitsDependingOnZPosition);

        public void SetCameraLimits(Bounds limits) =>
            cameraController.SetCameraLimits(limits);

        public void ConfigureZoom(float verticalCenterRef, float bottomMaxOffset, float topMaxOffset) =>
            cameraController.ConfigureZoom(verticalCenterRef, bottomMaxOffset, topMaxOffset);

        public void SetCharacterShadowActive(bool isActive) =>
            avatarShadow.SetActive(isActive);

        public void PlayEmote(string emoteId, long timestamp) =>
            avatar.PlayEmote(emoteId, timestamp);

        public void Dispose() =>
            Destroy(gameObject);
    }
}
