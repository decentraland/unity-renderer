using TMPro;
using UnityEngine;

public class TMPColorOnToggle : UIToggle
{
    [SerializeField] private TextMeshProUGUI targetText;

    [SerializeField] private Color onColor;

    [SerializeField] private Color offColor;

    protected override void OnValueChanged(bool isOn)
    {
        targetText.color = isOn ? onColor : offColor;
    }
}
