using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class UserBenchmarkToggler : MonoBehaviour
    {
        const string SHORTCUT_TEXT = "[Y] Debug Panel";

        public UserBenchmarkController userBenchmarkController;
        Text text;

        void Start()
        {
            text = GetComponent<Text>();
            text.text = SHORTCUT_TEXT;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Y))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            userBenchmarkController.gameObject.SetActive(!userBenchmarkController.gameObject.activeSelf);
            Utils.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }
    }
}