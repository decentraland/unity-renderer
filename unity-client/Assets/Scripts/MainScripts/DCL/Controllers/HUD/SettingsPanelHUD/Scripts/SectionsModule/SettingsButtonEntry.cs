using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Sections
{
    /// <summary>
    /// MonoBehaviour that represents a template for a menu button.
    /// It will be instantiated on the left part of the main settings panel and will be associated to a specific SECTION.
    /// </summary>
    public class SettingsButtonEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Color textColorOnSelect;

        [SerializeField]
        private Color backgroundColorOnSelect;

        private Color originalIconColor;
        private Color originalTextColor;
        private Color originalBackgroundColor;
        private bool isSelected;

        public void Initialize(Sprite icon, string text)
        {
            this.icon.sprite = icon;
            this.text.text = text;

            originalIconColor = this.icon.color;
            originalTextColor = this.text.color;
            originalBackgroundColor = backgroundImage.color;
        }

        /// <summary>
        /// Adds an action to execute when the button is clicked.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public void ConfigureAction(UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        /// <summary>
        /// Mark this button as selected. It will change the visual aspect of the button.
        /// </summary>
        /// <param name="isSelected">True for select the button.</param>
        public void MarkAsSelected(bool isSelected)
        {
            icon.color = isSelected ? textColorOnSelect : originalIconColor;
            text.color = isSelected ? textColorOnSelect : originalTextColor;
            backgroundImage.color = isSelected ? backgroundColorOnSelect : originalBackgroundColor;
            this.isSelected = isSelected;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isSelected)
                return;

            backgroundImage.color = backgroundColorOnSelect;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isSelected)
                return;

            backgroundImage.color = originalBackgroundColor;
        }
    }
}