using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.ProfanityFiltering;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

[Serializable]
public class PlayerName : MonoBehaviour, IPlayerName
{
    internal static readonly int TALKING_ANIMATOR_BOOL = Animator.StringToHash("Talking");
    internal const float MINIMUM_ALPHA_TO_SHOW = 1f / 32f;
    internal const float ALPHA_TRANSITION_STEP_PER_SECOND = 1f / 0.25f;
    internal const float ALPHA_STEPS = 32f;
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
    [SerializeField] internal Material nameMaterial;
    [SerializeField] internal Material nameOnTopMaterial;
    [SerializeField] internal Material bgOnTopMaterial;

    internal BaseVariable<float> namesOpacity => DataStore.i.HUDs.avatarNamesOpacity;
    internal BaseVariable<bool> namesVisible => DataStore.i.HUDs.avatarNamesVisible;
    internal BaseVariable<bool> profanityFilterEnabled;

    internal bool forceShow;
    internal Color backgroundOriginalColor;
    internal List<string> hideConstraints = new ();
    internal string currentName = "";

    private float alpha;
    private float targetAlpha;
    private bool renderersVisible;
    private bool isNameClaimed;
    private bool isUserGuest;
    private IProfanityFilter profanityFilter;

    // TODO: remove this property, is only used on tests
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

    // TODO: remove this property, is only used on tests
    internal float TargetAlpha
    {
        get => targetAlpha;
        set => targetAlpha = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        backgroundOriginalColor = background.color;
        profanityFilterEnabled = DataStore.i.settings.profanityChatFilteringEnabled;
        namesVisible.OnChange += OnNamesVisibleChanged;
        namesOpacity.OnChange += OnNamesOpacityChanged;
        profanityFilterEnabled.OnChange += OnProfanityFilterChanged;

        OnNamesVisibleChanged(namesVisible.Get(), true);
        OnNamesOpacityChanged(namesOpacity.Get(), 1);
        Show(true);
    }

    private void OnDestroy()
    {
        namesVisible.OnChange -= OnNamesVisibleChanged;
        namesOpacity.OnChange -= OnNamesOpacityChanged;
        profanityFilterEnabled.OnChange -= OnProfanityFilterChanged;
    }

    public void SetName(string name, bool isClaimed, bool isGuest) =>
        AsyncSetName(name, isClaimed, isGuest).Forget();

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
    public void AddVisibilityConstaint(string constraint)
    {
        hideConstraints.Add(constraint);
    }

    public void RemoveVisibilityConstaint(string constraint)
    {
        hideConstraints.Remove(constraint);
    }

    public void SetForceShow(bool forceShow)
    {
        canvas.enabled = forceShow || namesVisible.Get();

        background.color = new Color(backgroundOriginalColor.r, backgroundOriginalColor.g, backgroundOriginalColor.b, forceShow ? 1 : namesOpacity.Get());
        background.material = forceShow ? bgOnTopMaterial : null;
        nameText.fontSharedMaterial = forceShow ? nameOnTopMaterial : nameMaterial;
        this.forceShow = forceShow;

        if (this.forceShow)
            SetRenderersVisible(true);
    }

    public void SetIsTalking(bool talking)
    {
        talkingAnimator.SetBool(TALKING_ANIMATOR_BOOL, talking);
    }

    public void SetYOffset(float yOffset)
    {
        transform.localPosition = Vector3.up * yOffset;
    }

    public Rect ScreenSpaceRect(Camera mainCamera)
    {
        Vector3 origin = mainCamera.WorldToScreenPoint(background.transform.position);
        Vector2 size = background.rectTransform.sizeDelta;
        return new Rect(origin.x, Screen.height - origin.y, size.x, size.y);
    }

    public Vector3 ScreenSpacePos(Camera mainCamera)
    {
        return mainCamera.WorldToScreenPoint(background.transform.position);
    }

    internal void OnNamesOpacityChanged(float current, float previous)
    {
        background.color = new Color(backgroundOriginalColor.r, backgroundOriginalColor.g, backgroundOriginalColor.b, current);
    }

    internal void OnNamesVisibleChanged(bool current, bool previous)
    {
        canvas.enabled = current || forceShow;
    }

    private void OnProfanityFilterChanged(bool current, bool previous)
    {
        SetName(currentName, isNameClaimed, isUserGuest);
    }

    private async UniTaskVoid AsyncSetName(string name, bool isClaimed, bool isGuest)
    {
        currentName = name;
        isNameClaimed = isClaimed;
        isUserGuest = isGuest;

        if (string.IsNullOrEmpty(name))
        {
            background.rectTransform.sizeDelta = Vector3.zero;
            nameText.text = name;
            return;
        }

        name = await FilterName(currentName);
        nameText.text = GetNameWithColorCodes(isClaimed, isGuest, name);
        background.rectTransform.sizeDelta = new Vector2(nameText.GetPreferredValues().x + BACKGROUND_EXTRA_WIDTH, BACKGROUND_HEIGHT);
    }

    private async UniTask<string> FilterName(string name)
    {
        profanityFilter ??= Environment.i.serviceLocator.Get<IProfanityFilter>();

        return IsProfanityFilteringEnabled()
            ? await profanityFilter.Filter(name)
            : name;
    }

    private bool IsProfanityFilteringEnabled() =>
        DataStore.i.settings.profanityChatFilteringEnabled.Get();

    private void Update()
    {
        Update(Time.deltaTime);
    }

    private void SetRenderersVisible(bool value)
    {
        if (renderersVisible == value)
            return;

        canvasRenderers.ForEach(c => c.SetAlpha(value ? 1f : 0f));
        renderersVisible = value;
    }

    internal void Update(float deltaTime)
    {
        if (hideConstraints.Count > 0 || currentName == null)
        {
            UpdateVisuals(0);
            return;
        }

        float finalTargetAlpha = forceShow ? TARGET_ALPHA_SHOW : targetAlpha;
        alpha = Mathf.MoveTowards(alpha, finalTargetAlpha, ALPHA_TRANSITION_STEP_PER_SECOND * deltaTime);

        if (CheckAndUpdateVisibility(alpha))
            return;

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

        if (CheckAndUpdateVisibility(resolvedAlpha))
            return;

        SetRenderersVisible(true);
        float resolvedAlphaStep = GetNearestAlphaStep(resolvedAlpha);
        float canvasAlphaStep = GetNearestAlphaStep(canvasGroup.alpha);

        if (Math.Abs(resolvedAlphaStep - canvasAlphaStep) > Mathf.Epsilon)
            UpdateVisuals(resolvedAlpha);

        bool CheckAndUpdateVisibility(float checkAlpha)
        {
            if (checkAlpha >= MINIMUM_ALPHA_TO_SHOW) return false;
            if (!renderersVisible) return true;

            UpdateVisuals(0);
            SetRenderersVisible(false);

            return true;
        }
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

    internal float GetNearestAlphaStep(float alpha) =>
        Mathf.Floor(alpha * ALPHA_STEPS);

    internal void ScalePivotByDistance(float distanceToCamera)
    {
        pivot.transform.localScale = Vector3.one * 0.15f * distanceToCamera;
    }

    internal float GetPivotYOffsetByDistance(float distanceToCamera)
    {
        const float NEAR_Y_OFFSET = 0f;
        const float FAR_Y_OFFSET = 0.1f;
        const float MAX_DISTANCE = 5;

        if (distanceToCamera >= MAX_DISTANCE)
            return FAR_Y_OFFSET;

        return Mathf.Lerp(NEAR_Y_OFFSET, FAR_Y_OFFSET, distanceToCamera / MAX_DISTANCE);
    }

    private string GetNameWithColorCodes(bool isClaimed, bool isGuest, string name)
    {
        string text = name;

        if (isClaimed)
            text = $"<color=#FFFFFF>{name}</color>";
        else
        {
            if (isGuest)
            {
                string[] split = name.Split('#', StringSplitOptions.RemoveEmptyEntries);

                text = split.Length > 1
                    ? $"<color=#A09BA8>{split[0]}</color><color=#716B7C>#{split[1]}</color>"
                    : $"<color=#A09BA8>{name}</color>";
            }
            else
            {
                string[] split = name.Split('#');

                text = split.Length > 1
                    ? $"<color=#CFCDD4>{split[0]}</color><color=#A09BA8>#{split[1]}</color>"
                    : $"<color=#CFCDD4>{name}</color>";
            }
        }

        return text;
    }
}
