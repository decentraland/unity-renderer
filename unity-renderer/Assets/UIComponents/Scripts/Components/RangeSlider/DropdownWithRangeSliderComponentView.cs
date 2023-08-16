using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents.Scripts.Components.RangeSlider
{
    public class DropdownWithRangeSliderComponentView : BaseComponentView<RangeSliderComponentModel>
    {
        [Header("Prefab References")]
        [SerializeField] public RangeSliderComponentView slider;
        [SerializeField] internal Button button;
        [SerializeField] internal TMP_Text title;
        [SerializeField] internal GameObject contentPanel;
        [SerializeField] internal UIHelper_ClickBlocker blocker;

        public float MinValue => slider.MinValue;
        public float MaxValue => slider.MaxValue;
        public float LowValue => slider.LowValue;
        public float HighValue => slider.HighValue;
        public RangeSlider.RangeSliderEvent OnValueChanged => slider.OnValueChanged;

        private bool isOpen;

        public override void Awake()
        {
            base.Awake();

            RefreshControl();
            Close();

            blocker.OnClicked += Close;
            button.onClick.AddListener(() => ToggleOptionsList());
        }

        public override void Dispose()
        {
            base.Dispose();

            blocker.OnClicked -= Close;
            button.onClick.RemoveAllListeners();
        }

        public override void RefreshControl()
        {
            SetTitle(model.text);
            SetLimits(model.minValue, model.maxValue);
            SetWholeNumbers(model.wholeNumbers);
            SetValues(model.lowValue, model.highValue);
        }

        public void Open()
        {
            contentPanel.SetActive(true);
            isOpen = true;
            blocker.Activate();
        }

        public void Close()
        {
            contentPanel.SetActive(false);
            isOpen = false;
            blocker.Deactivate();
        }

        public void SetTitle(string newText)
        {
            if (title != null)
                title.text = newText;

            if (slider != null)
                slider.SetText(newText);
        }

        public void SetLimits(float minValue, float maxValue)
        {
            if (slider == null)
                return;

            slider.SetLimits(minValue, maxValue);
        }

        public void SetWholeNumbers(bool isWholeNumbers)
        {
            if (slider == null)
                return;

            slider.SetWholeNumbers(isWholeNumbers);
        }

        public void SetValues(float lowValue, float highValue)
        {
            if (slider == null)
                return;

            slider.SetValues(lowValue, highValue);
        }

        private void ToggleOptionsList()
        {
            if (isOpen)
                Close();
            else
                Open();
        }
    }
}
