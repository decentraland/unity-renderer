using System;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerName : MonoBehaviour, IPlayerName
{
    private const int DEFAULT_CANVAS_SORTING_ORDER = 0;
    private static readonly int TALKING_ANIMATOR_BOOL = Animator.StringToHash("Talking");
    private const float ALPHA_TRANSITION_STEP_PER_SECOND =  1f / 0.25f;
    private const float TARGET_ALPHA_SHOW = 1;
    private const float TARGET_ALPHA_HIDE = 0;

    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image background;
    [SerializeField] private Transform pivot;
    [SerializeField] private Animator talkingAnimator;

    private BaseVariable<float> namesOpacity => DataStore.i.HUDs.avatarNamesOpacity;
    private BaseVariable<bool> namesVisible => DataStore.i.HUDs.avatarNamesVisible;

    private float alpha;
    private float targetAlpha;
    private bool forceShow = false;
    private Color backgroundOriginalColor;

    private void Awake()
    {
        backgroundOriginalColor = background.color;
        alpha = 1;
        canvas.sortingOrder = DEFAULT_CANVAS_SORTING_ORDER;
        namesVisible.OnChange += OnNamesVisibleChanged;
        namesOpacity.OnChange += OnNamesOpacityChanged;
        OnNamesVisibleChanged(namesVisible.Get(), true);
        OnNamesOpacityChanged(namesOpacity.Get(), 1);
        Show(true);
    }
    private void OnNamesOpacityChanged(float current, float previous) { background.color = new Color(backgroundOriginalColor.r, backgroundOriginalColor.g, backgroundOriginalColor.b, current); }

    private void OnNamesVisibleChanged(bool current, bool previous) { canvas.enabled = current; }

    public void SetName(string name)
    {
        nameText.text = name;
        background.rectTransform.sizeDelta = new Vector2(nameText.GetPreferredValues().x + 50, 30);
    }

    private void Update()
    {
        float finalTargetAlpha = forceShow ? TARGET_ALPHA_SHOW : targetAlpha;

        if (!Mathf.Approximately(alpha, finalTargetAlpha))
            alpha = Mathf.MoveTowards(alpha, finalTargetAlpha, ALPHA_TRANSITION_STEP_PER_SECOND * Time.deltaTime);
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
        float distanceToPlayer = Vector3.Distance(cameraPosition, gameObject.transform.position);
        float resolvedAlpha = forceShow ? TARGET_ALPHA_SHOW : ResolveAlphaByDistance(alpha, distanceToPlayer, forceShow);
        UpdateVisuals(resolvedAlpha);
        ScalePivotByDistance(distanceToPlayer);
        LookAtCamera(cameraRight, cameraRotation.eulerAngles);
    }

    private void LookAtCamera(Vector3 cameraRight, Vector3 cameraEulerAngles)
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
        canvas.sortingOrder = forceShow ? int.MaxValue : DEFAULT_CANVAS_SORTING_ORDER;
        background.color = new Color(backgroundOriginalColor.r, backgroundOriginalColor.g, backgroundOriginalColor.b, forceShow ? 1 : namesOpacity.Get());
        this.forceShow = forceShow;
        if (this.forceShow)
            gameObject.SetActive(true);
    }

    public void SetIsTalking(bool talking) { talkingAnimator.SetBool(TALKING_ANIMATOR_BOOL, talking); }

    public Rect ScreenSpaceRect(Camera mainCamera)
    {
        Vector3 origin = mainCamera.WorldToScreenPoint(background.transform.position);
        Vector2 size = background.rectTransform.sizeDelta;
        return new Rect(origin.x, Screen.height - origin.y, size.x, size.y);
    }

    private static float ResolveAlphaByDistance(float alphaValue, float distanceToCamera, bool forceShow)
    {
        if (forceShow)
            return alphaValue;

        const float MIN_DISTANCE = 5;
        if (distanceToCamera < MIN_DISTANCE)
            return alphaValue;

        return alphaValue * Mathf.Lerp(1, 0, (distanceToCamera - MIN_DISTANCE) / (DataStore.i.avatarsLOD.LODDistance.Get() - MIN_DISTANCE));
    }

    private void UpdateVisuals(float resolvedAlpha) { canvasGroup.alpha = resolvedAlpha; }

    internal void ScalePivotByDistance(float distanceToCamera) { pivot.transform.localScale = Vector3.one * 0.15f * distanceToCamera; }

    private void OnDestroy()
    {
        namesVisible.OnChange -= OnNamesVisibleChanged;
        namesOpacity.OnChange -= OnNamesOpacityChanged;
    }
}