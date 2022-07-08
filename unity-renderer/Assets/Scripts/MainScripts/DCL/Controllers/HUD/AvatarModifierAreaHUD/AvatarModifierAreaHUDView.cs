using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModifierAreaHUDView : BaseComponentView
{

    public RectTransform warningMessageRectTransform;
    public AvatarModifierArea avatarModifierArea;
    private Coroutine warningMessageAnimationCoroutine;
    private CanvasGroup warningMessageGroup;
    private CanvasGroup warningIconCanvasGroup;
    private float animationDuration;
    public override void Awake()
    {
        base.Awake();
        warningMessageGroup = warningMessageRectTransform.GetComponent<CanvasGroup>();
        warningIconCanvasGroup = GetComponent<CanvasGroup>();
        DisableIcon();
        animationDuration = 0.5f;
    }

    public override void Start()
    {
        base.Start();
        avatarModifierArea.OnAvatarEnter += AvatarEntered;
        avatarModifierArea.OnAvatarExit += AvatarExit;
    }
    private void AvatarExit(GameObject obj)
    {
        DisableIcon();
    }
    private void AvatarEntered(GameObject obj)
    {
        EnableIcon();
    }

    private void DisableIcon()
    {
        warningIconCanvasGroup.alpha = 0;
        warningIconCanvasGroup.interactable = false;
        warningIconCanvasGroup.blocksRaycasts = false;
    }
    
    private void EnableIcon()
    {
        warningIconCanvasGroup.alpha = 0;
        warningIconCanvasGroup.interactable = false;
        warningIconCanvasGroup.blocksRaycasts = false;
    }

    public override void OnFocus()
    {
        base.OnFocus();
        if (warningMessageAnimationCoroutine != null)
        {
            StopCoroutine(warningMessageAnimationCoroutine);
        }
        warningMessageAnimationCoroutine = StartCoroutine(WarningAnimationCoroutine(Vector3.one, 1));
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        if (warningMessageAnimationCoroutine != null)
        {
            StopCoroutine(warningMessageAnimationCoroutine);
        }
        warningMessageAnimationCoroutine = StartCoroutine(WarningAnimationCoroutine(Vector3.zero, 0));
    }

    IEnumerator WarningAnimationCoroutine(Vector3 destinationScale, float destinationAlpha)
    {
        var t = 0f;
        Vector3 startScale = warningMessageRectTransform.localScale;
        float startAlphaMessage = warningMessageGroup.alpha;
        float startAlphaIcon = warningIconCanvasGroup.alpha;

        while (t < animationDuration)
        {
            t += Time.deltaTime;

            warningMessageRectTransform.localScale = Vector3.Lerp(startScale, destinationScale, t / animationDuration);
            warningMessageGroup.alpha = Mathf.Lerp(startAlphaMessage, destinationAlpha,  t / animationDuration);
            warningIconCanvasGroup.alpha = Mathf.Lerp(startAlphaIcon, 1 - destinationAlpha,  t / animationDuration);
            yield return null;
        }

        warningMessageRectTransform.localScale = destinationScale;
        warningMessageGroup.alpha = destinationAlpha;
    }

    public override void RefreshControl()
    {
        
    }

    public override void Dispose()
    {
        base.Dispose();
        avatarModifierArea.OnAvatarEnter -= AvatarEntered;
        avatarModifierArea.OnAvatarExit -= AvatarExit;
    }
}
