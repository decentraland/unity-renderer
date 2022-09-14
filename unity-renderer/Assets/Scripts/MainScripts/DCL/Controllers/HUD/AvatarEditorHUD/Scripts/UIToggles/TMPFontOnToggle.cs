using TMPro;
using UnityEngine;

[RequireComponent(typeof(UIOnToggleUpdater))]
public class TMPFontOnToggle : MonoBehaviour, IUIToggleBehavior
{
    [SerializeField] private TextMeshProUGUI targetText;

    [SerializeField] private TMP_FontAsset onFont;

    [SerializeField] private TMP_FontAsset offFont;

    public void Toggle(bool isOn) => targetText.font = isOn ? onFont : offFont;
}