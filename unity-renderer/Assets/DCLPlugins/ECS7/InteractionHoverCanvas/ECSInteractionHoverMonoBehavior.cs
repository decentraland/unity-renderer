using DCL.ECSComponents;
using UnityEngine;

public class ECSInteractionHoverMonoBehavior : MonoBehaviour, IECSInteractionHoverCanvas
{
    [SerializeField] internal ECSInteractionHoverTooltipMonoBehavior[] _tooltips;
    [SerializeField] internal Sprite[] _icons;
    [SerializeField] internal ShowHideAnimator _showHideAnimator;

    internal readonly string[] inputText = new[]
    {
        null, "E", "F", null,
        "W", "S", "D", "A",
        "SPACE BAR", "LSHIFT",
        "1", "2", "3", "4"
    };

    public int tooltipsCount => _tooltips.Length;

    public void Show()
    {
        _showHideAnimator.Show();
    }

    public void Hide()
    {
        _showHideAnimator.Hide();
    }

    public void SetTooltipInput(int tooltipIndex, ActionButton button)
    {
        switch (button)
        {
            case ActionButton.Pointer:
                _tooltips[tooltipIndex].SetInputIcon(_icons[0]);
                break;
            case ActionButton.Any:
                _tooltips[tooltipIndex].SetInputIcon(_icons[1]);
                break;
            default:
                int buttonIndex = (int)button;
                if (buttonIndex < inputText.Length && !string.IsNullOrEmpty(inputText[buttonIndex]))
                    _tooltips[tooltipIndex].SetInputText(inputText[buttonIndex]);
                break;
        }
    }

    public void SetTooltipText(int tooltipIndex, string text)
    {
        _tooltips[tooltipIndex].SetText(text);
    }

    public void SetTooltipActive(int tooltipIndex, bool active)
    {
        _tooltips[tooltipIndex].SetActive(active);
    }
}