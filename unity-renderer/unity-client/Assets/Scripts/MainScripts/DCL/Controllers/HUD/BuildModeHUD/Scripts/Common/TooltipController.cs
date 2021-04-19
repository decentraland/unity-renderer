using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ITooltipController
{
    void Initialize(ITooltipView view);
    void Dispose();
    void SetTooltipText(string text);
    void ShowTooltip(BaseEventData data);
    void ShowTooltip(BaseEventData data, Vector3 offset);
    void HideTooltip();
}

public class TooltipController : ITooltipController
{
    internal ITooltipView view;
    internal Coroutine changeAlphaCoroutine;

    public void Initialize(ITooltipView view)
    {
        this.view = view;

        view.OnShowTooltip += ShowTooltip;
        view.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        KillTooltipCoroutine();

        view.OnShowTooltip -= ShowTooltip;
        view.OnHideTooltip -= HideTooltip;
    }

    public void SetTooltipText(string text) { view.SetText(text); }

    public void ShowTooltip(BaseEventData data) { ShowTooltip(data, Vector3.zero); }

    public void ShowTooltip(BaseEventData data, Vector3 offset)
    {
        if (!(data is PointerEventData dataConverted))
            return;

        RectTransform selectedRT = dataConverted.pointerEnter.GetComponent<RectTransform>();
        view.SetTooltipPosition(offset + selectedRT.position - Vector3.up * selectedRT.rect.height);

        KillTooltipCoroutine();

        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(0, 1));
    }

    public void HideTooltip()
    {
        KillTooltipCoroutine();
        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(1, 0));
    }

    internal IEnumerator ChangeAlpha(float from, float to)
    {
        view.SetTooltipAlpha(from);

        float currentAlpha = from;
        float destinationAlpha = to;

        float fractionOfJourney = 0;
        float speed = view.alphaTranstionSpeed;
        while (fractionOfJourney < 1)
        {
            fractionOfJourney += Time.unscaledDeltaTime * speed;
            float lerpedAlpha = Mathf.Lerp(currentAlpha, destinationAlpha, fractionOfJourney);
            view.SetTooltipAlpha(lerpedAlpha);
            yield return null;
        }
        changeAlphaCoroutine = null;
    }

    internal void KillTooltipCoroutine()
    {
        if (changeAlphaCoroutine != null)
            CoroutineStarter.Stop(changeAlphaCoroutine);
    }
}