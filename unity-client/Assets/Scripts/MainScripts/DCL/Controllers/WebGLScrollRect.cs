using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class WebGLScrollRect : ScrollRect
{
    Vector2 fixedScrollDelta;

    // In the WebGL BUILD the horizontal scrolling on the scrollrect has a bug and is inverted
#if !UNITY_EDITOR && UNITY_WEBGL
    public override void OnScroll(PointerEventData eventData)
    {
        fixedScrollDelta.Set(-eventData.scrollDelta.x, eventData.scrollDelta.y);

        eventData.scrollDelta = fixedScrollDelta;
        base.OnScroll(eventData);
    }
#endif
}
