using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.ScreenshotCamera
{
    [RequireComponent(typeof(Canvas))] [DisallowMultipleComponent]
    public class ScreenshotHUDView : MonoBehaviour
    {
        [SerializeField] private Button shortcutButton;
        [SerializeField] private GameObject shortcutsHelpPanel;

        private Canvas canvas;

        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public Image RefImage { get; private set; }
        [field: SerializeField] public Button CloseButton { get; protected set; }
        [field: SerializeField] public Button TakeScreenshotButton { get; protected set; }

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            shortcutButton.onClick.AddListener(ToggleShortcutsHelpPanel);
        }

        public virtual void SwitchVisibility(bool isVisible) =>
            canvas.enabled = isVisible;

        private void ToggleShortcutsHelpPanel() =>
            shortcutsHelpPanel.SetActive(!shortcutsHelpPanel.activeSelf);
    }
}
