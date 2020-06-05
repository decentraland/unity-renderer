using UnityEngine;

namespace DCL.SettingsHUD
{
    public class SettingsHUDView : MonoBehaviour
    {
        public bool isOpen { get; private set; }

        private const string PATH = "SettingsHUD";

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
            gameObject.SetActive(visible);
            isOpen = visible;
        }
    }
}