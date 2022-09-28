using DCL.ECSComponents;
using UnityEngine;

public interface ITooltip
{
    void SetText(string text);
    void SetInputText(string text);
    void SetInputIcon(Sprite sprite);
    void SetActive(bool active);
}

public interface IECSInteractionHoverCanvas
{
    int tooltipsCount { get; }
    void Show();
    void Hide();
    void SetTooltipInput(int tooltipIndex, ActionButton button);
    void SetTooltipText(int tooltipIndex, string text);
    void SetTooltipActive(int tooltipIndex, bool active);
}