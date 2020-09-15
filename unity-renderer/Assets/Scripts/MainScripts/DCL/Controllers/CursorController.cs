using DCL;
using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public Image cursorImage;
    public Sprite normalCursor;
    public Sprite hoverCursor;

    void OnEnable()
    {
        Environment.i.pointerEventsController.OnPointerHoverStarts += SetHoverCursor;
        Environment.i.pointerEventsController.OnPointerHoverEnds += SetNormalCursor;
    }

    void OnDisable()
    {
        Environment.i.pointerEventsController.OnPointerHoverStarts -= SetHoverCursor;
        Environment.i.pointerEventsController.OnPointerHoverEnds -= SetNormalCursor;
    }

    void SetNormalCursor()
    {
        cursorImage.sprite = normalCursor;
        cursorImage.SetNativeSize();
    }

    void SetHoverCursor()
    {
        cursorImage.sprite = hoverCursor;
        cursorImage.SetNativeSize();
    }
}
