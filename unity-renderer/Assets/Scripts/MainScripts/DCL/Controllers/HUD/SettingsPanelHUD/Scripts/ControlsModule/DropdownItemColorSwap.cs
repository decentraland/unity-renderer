using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class DropdownItemColorSwap : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private Image targetBackground;
        [SerializeField] private Color textOn;
        [SerializeField] private Color textOff;
        [SerializeField] private Color bgOn;
        [SerializeField] private Color bgOff;
        
        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(toggle.isOn);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }
        
        private void OnValueChanged(bool value)
        {
            targetText.color = value ? textOn : textOff;
            targetBackground.color = value ? bgOn : bgOff;
        }
    }
}
