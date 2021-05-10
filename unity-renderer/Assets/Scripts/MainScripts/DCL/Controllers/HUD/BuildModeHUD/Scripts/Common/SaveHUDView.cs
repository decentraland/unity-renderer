using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ISaveHUDView
{
    void SetFirstPersonView();
    void SetGodModeView();
    void SceneStateSaved();
    void StopAnimation();
    void SetViewByEntityListOpen(bool isOpen);
}

public class SaveHUDView : MonoBehaviour, ISaveHUDView
{
    [Header("Animation design")]
    [SerializeField] internal float saveAnimationSpeed = 1.25f;
    [SerializeField] internal float sFullVisibleTime = 0.5f;
    [SerializeField] internal float percentMinVisible = 0.25f;
    [SerializeField] internal int totalAnimationTimes = 5;

    [Header("References")]
    [SerializeField] internal Image saveImg;
    [SerializeField] internal RectTransform godModePosition;
    [SerializeField] internal RectTransform godModeEntityListOpenPosition;
    [SerializeField] internal RectTransform firstPersonModePosition;
    [SerializeField] internal InspectorView inspectorView;

    private Coroutine animationCourutine;

    public void SceneStateSaved()
    {
        StopAnimation();
        animationCourutine =  CoroutineStarter.Start(SceneStateSaveAnimation());
    }
    public void StopAnimation()
    {
        if (animationCourutine != null)
        {
            CoroutineStarter.Stop(animationCourutine);
            animationCourutine = null;
        }
    }

    public void SetFirstPersonView() { saveImg.rectTransform.anchoredPosition = firstPersonModePosition.anchoredPosition; }

    public void SetGodModeView() { SetViewByEntityListOpen(inspectorView.IsActive()); }

    public void SetViewByEntityListOpen(bool isOpen) { saveImg.rectTransform.anchoredPosition = isOpen ? godModeEntityListOpenPosition.anchoredPosition : godModePosition.anchoredPosition; }

    IEnumerator SceneStateSaveAnimation()
    {
        float initialValue = percentMinVisible;
        float finalValue = 1;

        Color from = saveImg.color;
        from.a = initialValue;
        Color to = saveImg.color;
        to.a = finalValue;

        Color currentColor;

        float currentAdvance = 0;
        int currentAnimationTimes = 0;
        while (currentAnimationTimes < totalAnimationTimes)
        {
            currentAdvance += saveAnimationSpeed * Time.deltaTime;
            currentColor = Color.Lerp(from, to, currentAdvance);
            saveImg.color = currentColor;

            if (currentAdvance >= 1)
            {
                if (Mathf.Abs(from.a - initialValue) < 0.1f )
                {
                    from.a = finalValue;
                    to.a = initialValue;
                    yield return new WaitForSecondsRealtime(sFullVisibleTime);
                }
                else
                {
                    from.a = initialValue;
                    to.a = finalValue;
                }
                currentAnimationTimes++;
                currentAdvance = 0;
            }
            yield return null;
        }

        Color final = saveImg.color;
        final.a = 0;
        saveImg.color = final;
    }
}