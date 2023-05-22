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
        private const float CAMERA_TRANSITION_TIME = 0.3f;

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
        private (float minZ, float maxZ, float minY, float maxY, float minX, float maxX) cameraLimits;
        private (float verticalCenterRef, float bottomMaxOffset, float topMaxOffset) zoomConfig;

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

            SetFocus(CameraFocus.FaceSnapshot, false);
            avatarAnimator.Reset();
            yield return null;
            Texture2D face256 = Snapshot(SNAPSHOT_FACE_256_WIDTH_RES, SNAPSHOT_FACE_256_HEIGHT_RES);

            SetFocus(CameraFocus.BodySnapshot, false);
            avatarAnimator.Reset();
            yield return null;
            Texture2D body = Snapshot(SNAPSHOT_BODY_WIDTH_RES, SNAPSHOT_BODY_HEIGHT_RES);

            SetFocus(CameraFocus.DefaultEditing, false);

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

        public void SetFocus(CameraFocus focus, bool useTransition = true) =>
            SetFocus(cameraFocusLookUp[focus], useTransition);

        private void SetFocus(Transform transformToMove, bool useTransition = true)
        {
            if (cameraTransitionCoroutine != null) { StopCoroutine(cameraTransitionCoroutine); }

            if (useTransition && gameObject.activeInHierarchy)
            {
                cameraTransitionCoroutine = StartCoroutine(
                    CameraTransition(
                        cameraTransform.position, transformToMove.position,
                        cameraTransform.rotation, transformToMove.rotation,
                        CAMERA_TRANSITION_TIME));
            }
            else
            {
                cameraTransform.position = transformToMove.position;
                cameraTransform.rotation = transformToMove.rotation;
            }
        }

        private IEnumerator CameraTransition(Vector3 initPos, Vector3 endPos, Quaternion initRotation, Quaternion endRotation, float time)
        {
            float currentTime = 0;

            float inverseTime = 1 / time;

            while (currentTime < time)
            {
                currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0, time);
                cameraTransform.position = Vector3.Lerp(initPos, endPos, currentTime * inverseTime);
                cameraTransform.rotation = Quaternion.Lerp(initRotation, endRotation, currentTime * inverseTime);
                yield return null;
            }

            cameraTransitionCoroutine = null;
        }

        public void Rotate(float rotationVelocity) =>
            avatarContainer.transform.Rotate(Time.deltaTime * rotationVelocity * Vector3.up);

        public void ResetRotation() =>
            avatarContainer.transform.rotation = avatarContainerDefaultRotation;

        public void MoveCamera(Vector3 positionDelta, bool changeYLimitsDependingOnZPosition)
        {
            cameraTransform.Translate(positionDelta, Space.World);
            ApplyCameraLimits(changeYLimitsDependingOnZPosition);
        }

        public void SetCameraLimits(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            cameraLimits.minX = minX;
            cameraLimits.maxX = maxX;
            cameraLimits.minY = minY;
            cameraLimits.maxY = maxY;
            cameraLimits.minZ = minZ;
            cameraLimits.maxZ = maxZ;
        }

        public void ConfigureZoom(float verticalCenterRef, float bottomMaxOffset, float topMaxOffset)
        {
            zoomConfig.verticalCenterRef = verticalCenterRef;
            zoomConfig.bottomMaxOffset = bottomMaxOffset;
            zoomConfig.topMaxOffset = topMaxOffset;
        }

        private void ApplyCameraLimits(bool changeYLimitsDependingOnZPosition)
        {
            Vector3 pos = cameraTransform.localPosition;
            pos.x = Mathf.Clamp(pos.x, cameraLimits.minX, cameraLimits.maxX);
            pos.z = Mathf.Clamp(pos.z, cameraLimits.minZ, cameraLimits.maxZ);

            pos.y = changeYLimitsDependingOnZPosition ?
                GetCameraYPositionBasedOnZPosition() :
                Mathf.Clamp(pos.y, cameraLimits.minY, cameraLimits.maxY);

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
            if (zValue >= cameraLimits.maxZ) return 0f;
            if (zValue <= cameraLimits.minZ) return maxOffset;
            float progress = (zValue - cameraLimits.minZ) / (cameraLimits.maxZ - cameraLimits.minZ);
            return maxOffset - (progress * maxOffset);
        }

        public void SetCharacterShadowActive(bool isActive) =>
            avatarShadow.SetActive(isActive);

        public void PlayEmote(string emoteId, long timestamp) =>
            avatar.PlayEmote(emoteId, timestamp);

        public void Dispose() =>
            Destroy(gameObject);
    }
}
