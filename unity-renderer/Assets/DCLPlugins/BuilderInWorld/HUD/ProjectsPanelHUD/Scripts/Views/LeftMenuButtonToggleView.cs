using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Builder
{
    internal class LeftMenuButtonToggleView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public static event Action<LeftMenuButtonToggleView> OnToggleOn;

        [Header("Action")]
        [SerializeField] public SectionId openSection;

        [Header("Settings")]
        [SerializeField] private Color colorBackgroundDefault;
        [SerializeField] private Color colorBackgroundSelected;
        [SerializeField] private Color colorTextDefault;
        [SerializeField] private Color colorTextSelected;
        [SerializeField] private Color colorTextDisabled;

        [Header("References")]
        [SerializeField] private Image imageBackground;
        [SerializeField] private TextMeshProUGUI text;

        public bool isOn
        {
            set
            {
                if (isToggleOn == value)
                    return;

                SetIsOnWithoutNotify(value);

                if (value && !isDisabled)
                {
                    OnToggleOn?.Invoke(this);
                }
            }
            get { return isToggleOn; }
        }

        private bool isToggleOn = false;
        private bool isSetup = false;
        private bool isDisabled = false;

        public void Setup()
        {
            if (isSetup)
                return;

            isSetup = true;
            OnToggleOn += OnReceiveToggleOn;
        }

        public void Enable()
        {
            isDisabled = false;
            SetDefaultColor();
        }

        public void Disable()
        {
            isDisabled = true;
            SetDisableColor();
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            isToggleOn = value;

            if (isToggleOn)
            {
                SetSelectColor();
            }
            else
            {
                SetDefaultColor();
            }
        }

        private void OnDestroy() { OnToggleOn -= OnReceiveToggleOn; }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (isOn)
                return;

            SetSelectColor();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (isOn)
                return;

            SetDefaultColor();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            AudioScriptableObjects.buttonClick.Play(true);

            if (isOn)
                return;

            isOn = true;
        }

        private void SetSelectColor()
        {
            imageBackground.color = colorBackgroundSelected;
            text.color = colorTextSelected;
        }

        private void SetDisableColor() { text.color = colorTextDisabled; }

        private void SetDefaultColor()
        {
            imageBackground.color = colorBackgroundDefault;
            text.color = colorTextDefault;
        }

        private void OnReceiveToggleOn(LeftMenuButtonToggleView toggle)
        {
            if (!isOn)
                return;

            if (toggle != this)
            {
                isOn = false;
            }
        }
    }
}