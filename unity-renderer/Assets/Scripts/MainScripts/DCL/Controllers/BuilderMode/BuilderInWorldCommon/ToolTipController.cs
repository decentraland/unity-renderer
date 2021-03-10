using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipController : MonoBehaviour
{
    public float alphaSpeed = 3f;
    public RectTransform tooltipRT;
    public CanvasGroup tooltipCG;
    public TextMeshProUGUI tooltipTxt;

    Coroutine changeAlphaCoroutine;


    private void OnDestroy()
    {
        if (changeAlphaCoroutine != null)
            CoroutineStarter.Stop(changeAlphaCoroutine);
    }


    public void Stop()
    {
        if (changeAlphaCoroutine != null)
            CoroutineStarter.Stop(changeAlphaCoroutine);
        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(1, 0));
    }

    public void OnHoverEnter(BaseEventData data)
    {
        if (!(data is PointerEventData dataConverted))
            return;
        RectTransform selectedRT = dataConverted.pointerEnter.GetComponent<RectTransform>();
        
        tooltipRT.position = selectedRT.position-Vector3.up*selectedRT.rect.height;
        if (changeAlphaCoroutine != null)
            CoroutineStarter.Stop(changeAlphaCoroutine);
        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(0, 1));

    }

    public void SetText(string text)
    {
        tooltipTxt.text = text;
    }

    public void OnHoverExit()
    {
        Stop();
    }

    IEnumerator ChangeAlpha(float from, float to)
    {
        tooltipCG.alpha = from;

        float currentAlpha = from;
        float destinationAlpha = to;

        float fractionOfJourney = 0;
        float speed = alphaSpeed;
        while (fractionOfJourney < 1)
        {
            fractionOfJourney += Time.unscaledDeltaTime * speed;
            float lerpedAlpha = Mathf.Lerp(currentAlpha, destinationAlpha, fractionOfJourney);
            tooltipCG.alpha = lerpedAlpha;
            yield return null;
        }
        changeAlphaCoroutine = null;
    }
}
