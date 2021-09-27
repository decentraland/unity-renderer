using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class DropdownItemColorSwap : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI target;
        [SerializeField] private Color on;
        [SerializeField] private Color off;
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
            target.color = value ? on : off;
        }

    }
}