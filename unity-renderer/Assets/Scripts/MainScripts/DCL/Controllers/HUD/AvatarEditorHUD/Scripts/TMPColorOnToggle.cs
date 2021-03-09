using UnityEngine;
using UnityEngine.UI;

public class ImageColorOnToggle : UIToggle
{
    [SerializeField] private Image targetImage;

    [SerializeField] private Color onColor;

    [SerializeField] private Color offColor;

    protected override void OnValueChanged(bool isOn)
    {
        targetImage.color = isOn ? onColor : offColor;
    }
}