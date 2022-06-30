using System.Collections;
using UnityEngine;

public abstract class ChatEntry : MonoBehaviour
{
    [SerializeField] internal CanvasGroup group;
    protected bool fadeEnabled;
    protected Coroutine previewInterpolationRoutine;
    protected Coroutine previewInterpolationAlphaRoutine;
    
    public abstract ChatEntryModel Model { get; }
    public abstract void Populate(ChatEntryModel model);
    public abstract void SetFadeout(bool enabled);
    public abstract void DeactivatePreview(bool fadeOut);
    public abstract void ActivatePreview();
    public abstract void ActivatePreviewInstantly();
    public abstract void DeactivatePreviewInstantly();
    protected IEnumerator InterpolateAlpha(float destinationAlpha, float duration)
    {
        if (!fadeEnabled)
        {
            group.alpha = destinationAlpha;
            StopCoroutine(previewInterpolationAlphaRoutine); 
        }
        var t = 0f;
        var startAlpha = group.alpha;
        //NOTE(Brian): Small offset using normalized Y so we keep the cascade effect
        double yOffset = ((RectTransform) transform).anchoredPosition.y / (double) Screen.height * 4.0;
        duration -= (float) yOffset;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, destinationAlpha, t / duration);
            yield return null;
        }
        group.alpha = destinationAlpha;
    }
}