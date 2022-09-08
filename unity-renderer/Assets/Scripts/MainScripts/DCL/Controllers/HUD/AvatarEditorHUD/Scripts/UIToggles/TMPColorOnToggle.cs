using TMPro;
using UnityEngine;

[RequireComponent(typeof(UIOnToggleUpdater))]
public class TMPColorOnToggle : MonoBehaviour, IUIToggleBehavior
{
    [SerializeField] private TextMeshProUGUI targetText;

    [SerializeField] private Color onColor;

    [SerializeField] private Color offColor;

    public void Toggle(bool isOn) => targetText.color = isOn ? onColor : offColor;
}