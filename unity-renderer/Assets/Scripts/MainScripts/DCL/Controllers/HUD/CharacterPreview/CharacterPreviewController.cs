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

        private const int SUPERSAMPLING = 1;
        private const float CAMERA_TRANSITION_TIME = 0.5f;

        public delegate void OnSnapshotsReady(Texture2D face256, Texture2D body);

        public enum CameraFocus
        {
            DefaultEditing,
            FaceEditing,
            FaceSnapshot,
            BodySnapshot,
            Preview
        }

        private Dictionary<CameraFocus, Transform> cameraFocusLookUp;

        [SerializeField] private new Camera camera;
        [SerializeField] private Transform defaultEditingTemplate;
        [SerializeField] private Transform faceEditingTemplate;

        [SerializeField] private Transform faceSnapshotTemplate;
        [SerializeField] private Transform bodySnapshotTemplate;
        [SerializeField] private Transform previewTemplate;

        [SerializeField] private GameObject avatarContainer;
        [SerializeField] private GameObject avatarShadow;
        [SerializeField] private Transform baseAvatarContainer;
        [SerializeField] private BaseAvatarReferences baseAvatarReferencesPrefab;

        private Service<IAvatarFactory> avatarFactory;

        private IAvatar avatar;
        private readonly AvatarModel currentAvatarModel = new () { wearables = new List<string>() };
        private CancellationTokenSource loadingCts = new ();
        private IAnimator animator;
        private Quaternion avatarContainerDefaultRotation;
        private Transform cameraTransform;
        private float originalOrthographicSize;
        private float maxYOrthographicPosition;
        private float minYOrthographicPosition;

        private void Awake()
        {
            cameraFocusLookUp = new Dictionary<CameraFocus, Transform>()
            {
                { CameraFocus.DefaultEditing, defaultEditingTemplate },
                { CameraFocus.FaceEditing, faceEditingTemplate },
                { CameraFocus.FaceSnapshot, faceSnapshotTemplate },
                { CameraFocus.BodySnapshot, bodySnapshotTemplate },
                { CameraFocus.Preview, previewTemplate }
            };

            this.animator = GetComponentInChildren<IAnimator>();
            avatarContainerDefaultRotation = avatarContainer.transform.rotation;
            cameraTransform = camera.transform;
            originalOrthographicSize = camera.orthographicSize;
        }

        public void SetEnabled(bool isEnabled)
        {
            gameObject.SetActive(isEnabled);
            camera.enabled = isEnabled;
        }

        public void Initialize(CharacterPreviewMode loadingMode, RenderTexture targetTexture)
        {
            avatar?.Dispose();
            avatar = loadingMode switch
                     {
                         CharacterPreviewMode.WithHologram => CreateAvatarWithHologram(),
                         CharacterPreviewMode.WithoutHologram => CreateAvatar(),
                         _ => avatar,
                     };

            camera.targetTexture = targetTexture;
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
                List<string> wearables = new List<string>(newModel.wearables);
                wearables.Add(newModel.bodyShape);

                await avatar.Load(wearables, newModel.emotes.Select(x => x.urn).ToList(), new AvatarSettings
                {
                    bodyshapeId = newModel.bodyShape,
                    eyesColor = newModel.eyeColor,
                    hairColor = newModel.hairColor,
                    skinColor = newModel.skinColor
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

            var current = camera.targetTexture;
            camera.targetTexture = null;
            var avatarAnimator = avatarContainer.gameObject.GetComponentInChildren<AvatarAnimatorLegacy>();

            SetFocus(CameraFocus.FaceSnapshot, null, false);
            avatarAnimator.Reset();
            yield return null;
            Texture2D face256 = Snapshot(SNAPSHOT_FACE_256_WIDTH_RES, SNAPSHOT_FACE_256_HEIGHT_RES);

            SetFocus(CameraFocus.BodySnapshot, null, false);
            avatarAnimator.Reset();
            yield return null;
            Texture2D body = Snapshot(SNAPSHOT_BODY_WIDTH_RES, SNAPSHOT_BODY_HEIGHT_RES);

            SetFocus(CameraFocus.DefaultEditing, null, false);

            camera.targetTexture = current;

            global::DCL.Environment.i.platform.cullingController.Start();
            callback?.Invoke(face256, body);
        }

        private Texture2D Snapshot(int width, int height)
        {
            RenderTexture rt = new RenderTexture(width * SUPERSAMPLING, height * SUPERSAMPLING, 32);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenShot.Apply();

            return screenShot;
        }

        private Coroutine cameraTransitionCoroutine;

        public void SetFocus(CameraFocus focus, float? orthographicSize = null, bool useTransition = true) =>
            SetFocus(cameraFocusLookUp[focus], orthographicSize, useTransition);

        private void SetFocus(Transform transformToMove, float? orthographicSize = null, bool useTransition = true)
        {
            if (cameraTransitionCoroutine != null) { StopCoroutine(cameraTransitionCoroutine); }

            if (useTransition && gameObject.activeInHierarchy)
            {
                float currentCameraOrthographicSize = camera.orthographicSize;

                cameraTransitionCoroutine = StartCoroutine(
                    CameraTransition(
                        cameraTransform.position, transformToMove.position,
                        cameraTransform.rotation, transformToMove.rotation,
                        currentCameraOrthographicSize, orthographicSize ?? originalOrthographicSize,
                        CAMERA_TRANSITION_TIME));
            }
            else
            {
                cameraTransform.position = transformToMove.position;
                cameraTransform.rotation = transformToMove.rotation;
            }
        }

        private IEnumerator CameraTransition(Vector3 initPos, Vector3 endPos, Quaternion initRotation, Quaternion endRotation, float initOrthographicSize, float endOrthographicSize, float time)
        {
            float currentTime = 0;

            float inverseTime = 1 / time;

            while (currentTime < time)
            {
                currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0, time);
                cameraTransform.position = Vector3.Lerp(initPos, endPos, currentTime * inverseTime);
                cameraTransform.rotation = Quaternion.Lerp(initRotation, endRotation, currentTime * inverseTime);
                camera.orthographicSize = Mathf.Lerp(initOrthographicSize, endOrthographicSize, currentTime * inverseTime);
                yield return null;
            }

            cameraTransitionCoroutine = null;
        }

        public void Rotate(float rotationVelocity) =>
            avatarContainer.transform.Rotate(Time.deltaTime * rotationVelocity * Vector3.up);

        public void ResetRotation() =>
            avatarContainer.transform.rotation = avatarContainerDefaultRotation;

        public void MoveCamera(Vector3 positionDelta)
        {
            cameraTransform.Translate(positionDelta, Space.World);
            LimitCameraPositionDependingOnOrthographicSize();
        }

        public void SetCameraProjection(bool isOrthographic) =>
            camera.orthographic = isOrthographic;

        public void SetOrthographicLimits(float minY, float maxY)
        {
            minYOrthographicPosition = minY;
            maxYOrthographicPosition = maxY;
        }

        public void SetCameraOrthographicSize(float orthographicSizeDelta, float minOrthographicSize, float maxOrthographicSize)
        {
            float orthographicSize = camera.orthographicSize;
            orthographicSize = Mathf.Clamp(orthographicSize + orthographicSizeDelta, minOrthographicSize, maxOrthographicSize);
            camera.orthographicSize = orthographicSize;
            LimitCameraPositionDependingOnOrthographicSize();
        }

        public void SetCharacterShadowActive(bool isActive) =>
            avatarShadow.SetActive(isActive);

        private void LimitCameraPositionDependingOnOrthographicSize()
        {
            if (!camera.orthographic)
                return;

            Vector3 cameraPosition = cameraTransform.position;
            float cameraProjectionHeight = camera.orthographicSize * 2f;
            float topYProjectionPosition = cameraPosition.y + (cameraProjectionHeight * 0.5f);
            float bottomYProjectionPosition = cameraPosition.y - (cameraProjectionHeight * 0.5f);

            if (topYProjectionPosition > maxYOrthographicPosition)
                cameraPosition.y = maxYOrthographicPosition - (cameraProjectionHeight * 0.5f);
            else if (bottomYProjectionPosition < minYOrthographicPosition)
                cameraPosition.y = minYOrthographicPosition + (cameraProjectionHeight * 0.5f);

            cameraTransform.position = cameraPosition;
        }

        public void PlayEmote(string emoteId, long timestamp) =>
            avatar.PlayEmote(emoteId, timestamp);

        public void Dispose() =>
            Destroy(gameObject);
    }
}
