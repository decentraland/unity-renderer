using DCL;
using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public static CursorController i { get; private set; }
    public Image cursorImage;
    public Sprite normalCursor;
    public Sprite hoverCursor;

    void Awake()
    {
        i = this;
    }

    public void SetNormalCursor()
    {
        cursorImage.sprite = normalCursor;
        cursorImage.SetNativeSize();
    }

    public void SetHoverCursor()
    {
        cursorImage.sprite = hoverCursor;
        cursorImage.SetNativeSize();
    }
}