using System;
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

    internal BaseVariable<float> namesOpacity => DataStore.i.HUDs.avatarNamesOpacity;
    internal BaseVariable<bool> namesVisible => DataStore.i.HUDs.avatarNamesVisible;

    internal float alpha;
    internal float targetAlpha;
    internal bool forceShow = false;
    internal Color backgroundOriginalColor;

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

    public void SetName(string name)
    {
        name = ProfanityFilterSharedInstances.regexFilter.Filter(name);
        nameText.text = name;
        background.rectTransform.sizeDelta = new Vector2(nameText.GetPreferredValues().x + BACKGROUND_EXTRA_WIDTH, BACKGROUND_HEIGHT);
    }

    private void Update() { Update(Time.deltaTime); }

    internal void Update(float deltaTime)
    {
        float finalTargetAlpha = forceShow ? TARGET_ALPHA_SHOW : targetAlpha;

        if (!Mathf.Approximately(alpha, finalTargetAlpha))
            alpha = Mathf.MoveTowards(alpha, finalTargetAlpha, ALPHA_TRANSITION_STEP_PER_SECOND * deltaTime);
        else if (alpha == 0)
        {
            UpdateVisuals(0);
            // We are hidden and we dont have to scale, look at camera or anything, we can disable the gameObject
            gameObject.SetActive(false);
            return;
        }
        Vector3 cameraPosition = CommonScriptableObjects.cameraPosition.Get();
        Vector3 cameraRight = CommonScriptableObjects.cameraRight.Get();
        Quaternion cameraRotation = DataStore.i.camera.rotation.Get();

        /*
         * TODO: We could obtain distance to player from the AvatarLODController but that coupling it's overkill and ugly
         * instead we should have a provider so all the subsystems can use it
         */
        float distanceToCamera = Vector3.Distance(cameraPosition, gameObject.transform.position);
        float resolvedAlpha = forceShow ? TARGET_ALPHA_SHOW : ResolveAlphaByDistance(alpha, distanceToCamera, forceShow);
        UpdateVisuals(resolvedAlpha);
        ScalePivotByDistance(distanceToCamera);
        LookAtCamera(cameraRight, cameraRotation.eulerAngles);
        pivot.transform.localPosition = Vector3.up * GetPivotYOffsetByDistance(distanceToCamera);
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
        gameObject.SetActive(true);
        if (instant)
            alpha = TARGET_ALPHA_SHOW;
    }

    public void Hide(bool instant = false)
    {
        targetAlpha = TARGET_ALPHA_HIDE;
        if (instant && !forceShow)
        {
            alpha = TARGET_ALPHA_HIDE;
            gameObject.SetActive(false);
        }
    }
    public void SetForceShow(bool forceShow)
    {
        canvas.enabled = forceShow || namesVisible.Get();
        canvas.sortingOrder = forceShow ? FORCE_CANVAS_SORTING_ORDER : DEFAULT_CANVAS_SORTING_ORDER;
        background.color = new Color(backgroundOriginalColor.r, backgroundOriginalColor.g, backgroundOriginalColor.b, forceShow ? 1 : namesOpacity.Get());
        this.forceShow = forceShow;
        if (this.forceShow)
            gameObject.SetActive(true);
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

    internal void UpdateVisuals(float resolvedAlpha) { canvasGroup.alpha = resolvedAlpha; }

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