using System;
using System.Collections;
using TMPro;
using UnityEngine;

[Serializable]
public class PlayerName : MonoBehaviour, IPlayerName
{
    private const float ALPHA_TRANSITION_STEP_PER_SECOND =  1f / 0.25f;
    private const float TARGET_ALPHA_SHOW = 1;
    private const float TARGET_ALPHA_HIDE = 0;

    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private RectTransform background;
    [SerializeField] private Transform pivot;

    private float alpha;
    private float targetAlpha;
    private bool forceShow = false;

    private void Awake()
    {
        alpha = 1;
        Show(true);
    }

    public void SetName(string name)
    {
        nameText.text = name;
        background.sizeDelta = new Vector2(nameText.GetPreferredValues().x * 1.25f, 30);
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

        /*
         * TODO: We could obtain distance to player from the AvatarLODController but that coupling it's overkill and ugly
         * instead we should have a provider so all the subsystems can use it
         */
        float distanceToPlayer = Vector3.Distance(cameraPosition, gameObject.transform.position);
        float resolvedAlpha = ResolveAlphaByDistance(alpha, distanceToPlayer, forceShow);
        UpdateVisuals(resolvedAlpha);
        ScalePivotByDistance(distanceToPlayer);

        transform.LookAt(cameraPosition);
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
        canvas.sortingOrder = forceShow ? int.MaxValue : 0;
        this.forceShow = forceShow;
        if (this.forceShow)
            gameObject.SetActive(true);
    }

    private static float ResolveAlphaByDistance(float alphaValue, float distanceToCamera, bool forceShow)
    {
        if (forceShow)
            return alphaValue;

        const float MIN_VALUE = 5;
        if (distanceToCamera < MIN_VALUE)
            return alphaValue;

        return alphaValue * Mathf.Lerp(1, 0, (distanceToCamera - MIN_VALUE) / (15 - MIN_VALUE));
    }

    private void UpdateVisuals(float resolvedAlpha) { canvasGroup.alpha = resolvedAlpha; }

    internal void ScalePivotByDistance(float distanceToCamera) { pivot.transform.localScale = Vector3.one * 0.1f * distanceToCamera; }
}