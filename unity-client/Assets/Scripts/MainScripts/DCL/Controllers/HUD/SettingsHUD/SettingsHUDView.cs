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
            if (HUDAudioPlayer.i != null)
            {
                if (visible && !isOpen)
                    HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.dialogAppear);
                else if (isOpen)
                    HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.dialogClose);
            }

            gameObject.SetActive(visible);
            isOpen = visible;
        }
    }
}