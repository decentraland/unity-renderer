using UnityEngine;

public interface IBackpackSectionComponentView
{
    void EncapsulateAvatarEditorHUD(AvatarEditorHUDView view);
}

public class BackpackSectionComponentView : BaseComponentView, IBackpackSectionComponentView
{
    public override void RefreshControl() { }

    public void EncapsulateAvatarEditorHUD(AvatarEditorHUDView view)
    {
        view.transform.SetParent(transform);
        view.transform.localScale = Vector3.one;

        RectTransform rectTransform = view.transform as RectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localPosition = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
    }
}