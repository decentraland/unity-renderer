using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIOnToggleUpdater))]
public class ImageColorOnToggle : MonoBehaviour, IUIToggleBehavior
{
    [SerializeField] private Image targetImage;

    [SerializeField] private Color onColor;

    [SerializeField] private Color offColor;

    public void Toggle(bool isOn) => targetImage.color = isOn ? onColor : offColor;
}