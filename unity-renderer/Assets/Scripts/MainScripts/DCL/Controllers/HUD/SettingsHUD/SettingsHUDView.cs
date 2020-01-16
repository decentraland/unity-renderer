using UnityEngine;

namespace DCL.SettingsHUD
{
    public class SettingsHUDView : MonoBehaviour
    {
        private const string PATH = "SettingsHUD";

        public static SettingsHUDView Create()
        {
            SettingsHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<SettingsHUDView>();
            view.name = "_SettingsHUD";
            return view;
        }

        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}