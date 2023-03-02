using UnityEngine.UIElements;

public static class UiElementUtils
{
    public static void SetElementDefaultStyle(IStyle elementStyle)
    {
        elementStyle.right = 0;
        elementStyle.left = 0;
        elementStyle.top = 0;
        elementStyle.bottom = 0;
        elementStyle.width = new StyleLength(StyleKeyword.Auto);
        elementStyle.height = new StyleLength(StyleKeyword.Auto);
        elementStyle.position = new StyleEnum<Position>(Position.Absolute);
        elementStyle.justifyContent = new StyleEnum<Justify>(Justify.Center);
        elementStyle.alignItems = new StyleEnum<Align>(Align.Center);
    }
}
