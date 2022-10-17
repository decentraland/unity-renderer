using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using GPUSkinning;
using UnityEngine;
using Environment = DCL.Environment;

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
        BodySnapshot
    }

    private System.Collections.Generic.Dictionary<CameraFocus, Transform> cameraFocusLookUp;

    public new Camera camera;

    public Transform defaultEditingTemplate;
    public Transform faceEditingTemplate;

    public Transform faceSnapshotTemplate;
    public Transform bodySnapshotTemplate;

    [SerializeField] private GameObject avatarContainer;
    [SerializeField] private Transform avatarRevealContainer;
    private IAvatar avatar;
    private readonly AvatarModel currentAvatarModel = new AvatarModel { wearables = new List<string>() };
    private CancellationTokenSource loadingCts = new CancellationTokenSource();
    

    private void Awake()
    {
        cameraFocusLookUp = new Dictionary<CameraFocus, Transform>()
        {
            { CameraFocus.DefaultEditing, defaultEditingTemplate },
            { CameraFocus.FaceEditing, faceEditingTemplate },
            { CameraFocus.FaceSnapshot, faceSnapshotTemplate },
            { CameraFocus.BodySnapshot, bodySnapshotTemplate },
        };
        IAnimator animator = avatarContainer.gameObject.GetComponentInChildren<IAnimator>();
        avatar = new AvatarSystem.Avatar(
            new AvatarCurator(new WearableItemResolver(), Environment.i.serviceLocator.Get<IEmotesCatalogService>()),
            new Loader(new WearableLoaderFactory(), avatarContainer, new AvatarMeshCombinerHelper()),
            animator,
            new Visibility(),
            new NoLODs(),
            new SimpleGPUSkinning(),
            new GPUSkinningThrottler(),
            new EmoteAnimationEquipper(animator, DataStore.i.emotes)
        ) ;
    }

    public void UpdateModel(AvatarModel newModel,Action onDone)
    {
        if (newModel.HaveSameWearablesAndColors(currentAvatarModel))
        {
            onDone?.Invoke();
            return;
        }

        loadingCts?.Cancel();
        loadingCts?.Dispose();
        loadingCts = new CancellationTokenSource();
        UpdateModelRoutine(newModel, onDone, loadingCts.Token);
    }

    private void OnDestroy()
    {
        loadingCts?.Cancel();
        loadingCts?.Dispose();
        loadingCts = null;
        avatar?.Dispose();
    }

    private async UniTaskVoid UpdateModelRoutine(AvatarModel newModel, Action onDone, CancellationToken ct)
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
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }
        onDone?.Invoke();
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
        DCL.Environment.i.platform.cullingController.Stop();

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

        DCL.Environment.i.platform.cullingController.Start();
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

    public void SetFocus(CameraFocus focus, bool useTransition = true) { SetFocus(cameraFocusLookUp[focus], useTransition); }

    private void SetFocus(Transform transform, bool useTransition = true)
    {
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }

        if (useTransition)
        {
            cameraTransitionCoroutine = StartCoroutine(CameraTransition(camera.transform.position, transform.position, camera.transform.rotation, transform.rotation, CAMERA_TRANSITION_TIME));
        }
        else
        {
            var cameraTransform = camera.transform;
            cameraTransform.position = transform.position;
            cameraTransform.rotation = transform.rotation;
        }
    }

    private IEnumerator CameraTransition(Vector3 initPos, Vector3 endPos, Quaternion initRotation, Quaternion endRotation, float time)
    {
        var cameraTransform = camera.transform;
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

    public void Rotate(float rotationVelocity) { avatarContainer.transform.Rotate(Time.deltaTime * rotationVelocity * Vector3.up); }

    public AvatarModel GetCurrentModel() { return currentAvatarModel; }

    public void PlayEmote(string emoteId, long timestamp) { avatar.PlayEmote(emoteId, timestamp); }
}