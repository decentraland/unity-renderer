using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerName : MonoBehaviour, IPlayerName
{
    internal const int DEFAULT_CANVAS_SORTING_ORDER = 0;
    internal const int FORCE_CANVAS_SORTING_ORDER = 40;
    internal static readonly int TALKING_ANIMATOR_BOOL = Animator.StringToHash("Talking");
    internal const float ALPHA_TRANSITION_STEP_PER_SECOND =  1f / 0.25f;
    internal const float ALPHA_STEPS = 16f;
    internal const float TARGET_ALPHA_SHOW = 1;
    internal const float TARGET_ALPHA_HIDE = 0;
    internal const int BACKGROUND_HEIGHT = 30;
    internal const int BACKGROUND_EXTRA_WIDTH = 10;

    [SerializeField] internal Canvas canvas;
    [SerializeField] internal CanvasGroup canvasGroup;
    [SerializeField] internal TextMeshProUGUI nameText;
    [SerializeField] internal Image background;
    [SerializeField] internal Transform pivot;
    [SerializeField] internal Animator talkingAnimator;
    
    [SerializeField] internal List<CanvasRenderer> canvasRenderers;

    internal BaseVariable<float> namesOpacity => DataStore.i.HUDs.avatarNamesOpacity;
    internal BaseVariable<bool> namesVisible => DataStore.i.HUDs.avatarNamesVisible;

    internal bool forceShow = false;
    internal Color backgroundOriginalColor;
    internal List<string> hideConstraints = new List<string>();

    private float alpha;
    private float targetAlpha;


    internal float Alpha 
    {
        get => alpha;
        set
        {
            alpha = Mathf.Clamp01(value);
            if (alpha < 0.01f)
            {
                UpdateVisuals(0);
                SetRenderersVisible(false);
                return;
            }

            UpdateVisuals(alpha);
            SetRenderersVisible(true);
        }
    }

    internal float TargetAlpha
    {
        get => targetAlpha;
        set => targetAlpha = Mathf.Clamp01(value);
    }


    private void Awake()
    {
        backgroundOriginalColor = background.color;
        canvas.sortingOrder = DEFAULT_CANVAS_SORTING_ORDER;
        namesVisible.OnChange += OnNamesVisibleChanged;
        namesOpacity.OnChange += OnNamesOpacityChanged;
        OnNamesVisibleChanged(namesVisible.Get(), true);
        OnNamesOpacityChanged(namesOpacity.Get(), 1);
        Show(true);
    }
    internal void OnNamesOpacityChanged(float current, float previous) { background.color = new Color(backgroundOriginalColor.r, backgroundOriginalColor.g, backgroundOriginalColor.b, current); }

    internal void OnNamesVisibleChanged(bool current, bool previous) { canvas.enabled = current || forceShow; }

    public void SetName(string name) { AsyncSetName(name).Forget(); }
    private async UniTaskVoid AsyncSetName(string name)
    {
        name = await ProfanityFilterSharedInstances.regexFilter.Filter(name);
        nameText.text = name;
        background.rectTransform.sizeDelta = new Vector2(nameText.GetPreferredValues().x + BACKGROUND_EXTRA_WIDTH, BACKGROUND_HEIGHT);
    }

    private void Update() { Update(Time.deltaTime); }

    private void SetRenderersVisible(bool value)
    {
        canvasRenderers.ForEach(c => c.SetAlpha(value ? 1f : 0f));
    }

    internal void Update(float deltaTime)
    {
        if (hideConstraints.Count > 0)
        {
            UpdateVisuals(0);
            return;
        }

        float previousAlphaStep = GetNearestAlphaStep(alpha);
        float finalTargetAlpha = forceShow ? TARGET_ALPHA_SHOW : targetAlpha;
        alpha = Mathf.MoveTowards(alpha, finalTargetAlpha, ALPHA_TRANSITION_STEP_PER_SECOND * deltaTime);
        float currentAlphaStep = GetNearestAlphaStep(alpha);

        if (currentAlphaStep == 0)
        {
            if (previousAlphaStep != currentAlphaStep)
            {
                UpdateVisuals(0);
                SetRenderersVisible(false);
            }
            return;
        }
        else
            SetRenderersVisible(true);


        Vector3 cameraPosition = CommonScriptableObjects.cameraPosition.Get();
        Vector3 cameraRight = CommonScriptableObjects.cameraRight.Get();
        Quaternion cameraRotation = DataStore.i.camera.rotation.Get();

        /*
         * TODO: We could obtain distance to player from the AvatarLODController but that coupling it's overkill and ugly
         * instead we should have a provider so all the subsystems can use it
         */
        float distanceToCamera = Vector3.Distance(cameraPosition, gameObject.transform.position);
        ScalePivotByDistance(distanceToCamera);
        LookAtCamera(cameraRight, cameraRotation.eulerAngles);
        pivot.transform.localPosition = Vector3.up * GetPivotYOffsetByDistance(distanceToCamera);

        float resolvedAlpha = forceShow ? TARGET_ALPHA_SHOW : ResolveAlphaByDistance(alpha, distanceToCamera, forceShow);
        float resolvedAlphaStep = GetNearestAlphaStep(resolvedAlpha);
        float canvasAlphaStep = GetNearestAlphaStep(canvasGroup.alpha);
        if (resolvedAlphaStep != canvasAlphaStep)
            UpdateVisuals(resolvedAlpha);
    }

    internal void LookAtCamera(Vector3 cameraRight, Vector3 cameraEulerAngles)
    {
        Transform cachedTransform = transform;
        cachedTransform.right = -cameraRight; // This will set the Y rotation

        // Now we use the negated X axis rotation to make the rotation steady in horizont
        Vector3 finalEulerAngle = cachedTransform.localEulerAngles;
        finalEulerAngle.x = -cameraEulerAngles.x;
        cachedTransform.localEulerAngles = finalEulerAngle;
    }

    public void Show(bool instant = false)
    {
        targetAlpha = TARGET_ALPHA_SHOW;
        SetRenderersVisible(true);
        if (instant)
            alpha = TARGET_ALPHA_SHOW;
    }

    public void Hide(bool instant = false)
    {
        targetAlpha = TARGET_ALPHA_HIDE;
        if (instant && !forceShow)
        {
            alpha = TARGET_ALPHA_HIDE;
            SetRenderersVisible(false);
        }
    }

    // Note: Ideally we should separate the LODController logic from this one so we don't have to add constraints
    public void AddVisibilityConstaint(string constraint) { hideConstraints.Add(constraint); }
    
    public void RemoveVisibilityConstaint(string constraint) { hideConstraints.Remove(constraint); }

    public void SetForceShow(bool forceShow)
    {
        canvas.enabled = forceShow || namesVisible.Get();
        canvas.sortingOrder = forceShow ? FORCE_CANVAS_SORTING_ORDER : DEFAULT_CANVAS_SORTING_ORDER;
        background.color = new Color(backgroundOriginalColor.r, backgroundOriginalColor.g, backgroundOriginalColor.b, forceShow ? 1 : namesOpacity.Get());
        this.forceShow = forceShow;
        if (this.forceShow)
            SetRenderersVisible(true);
    }

    public void SetIsTalking(bool talking) { talkingAnimator.SetBool(TALKING_ANIMATOR_BOOL, talking); }

    public void SetYOffset(float yOffset) { transform.localPosition = Vector3.up * yOffset; }

    public Rect ScreenSpaceRect(Camera mainCamera)
    {
        Vector3 origin = mainCamera.WorldToScreenPoint(background.transform.position);
        Vector2 size = background.rectTransform.sizeDelta;
        return new Rect(origin.x, Screen.height - origin.y, size.x, size.y);
    }

    public Vector3 ScreenSpacePos(Camera mainCamera) { return mainCamera.WorldToScreenPoint(background.transform.position); }

    internal static float ResolveAlphaByDistance(float alphaValue, float distanceToCamera, bool forceShow)
    {
        if (forceShow)
            return alphaValue;

        const float MIN_DISTANCE = 5;
        if (distanceToCamera < MIN_DISTANCE)
            return alphaValue;

        return alphaValue * Mathf.Lerp(1, 0, (distanceToCamera - MIN_DISTANCE) / (DataStore.i.avatarsLOD.LODDistance.Get() - MIN_DISTANCE));
    }

    internal void UpdateVisuals(float resolvedAlpha)
    {
        canvasGroup.alpha = resolvedAlpha;
    }

    internal float GetNearestAlphaStep(float alpha)
    {
        return Mathf.Floor(alpha * ALPHA_STEPS) / ALPHA_STEPS;
    }

    internal void ScalePivotByDistance(float distanceToCamera) { pivot.transform.localScale = Vector3.one * 0.15f * distanceToCamera; }

    internal float GetPivotYOffsetByDistance(float distanceToCamera)
    {
        const float NEAR_Y_OFFSET = 0f;
        const float FAR_Y_OFFSET = 0.1f;
        const float MAX_DISTANCE = 5;
        if (distanceToCamera >= MAX_DISTANCE)
            return FAR_Y_OFFSET;

        return Mathf.Lerp(NEAR_Y_OFFSET, FAR_Y_OFFSET, distanceToCamera / MAX_DISTANCE);
    }

    private void OnDestroy()
    {
        namesVisible.OnChange -= OnNamesVisibleChanged;
        namesOpacity.OnChange -= OnNamesOpacityChanged;
    }
}