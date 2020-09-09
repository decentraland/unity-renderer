using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsHUD
{
    public class SettingsHUDView : MonoBehaviour
    {
        [SerializeField] private ShowHideAnimator settingsAnimator;

        public bool isOpen { get; private set; }

        private const string PATH = "SettingsHUD";

        [SerializeField] internal Button closeButton;
        [SerializeField] internal Button doneButton;

        private void Awake()
        {
            isOpen = false;
        }

        public static SettingsHUDView Create()
        {
            SettingsHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<SettingsHUDView>();
            view.name = "_SettingsHUD";
            return view;
        }

        public void SetVisibility(bool visible)
        {
            if (visible && !isOpen)
                AudioScriptableObjects.dialogOpen.Play(true);
            else if (isOpen)
                AudioScriptableObjects.dialogClose.Play(true);

            if (visible)
                settingsAnimator.Show();
            else
                settingsAnimator.Hide();

            isOpen = visible;
        }
    }
}