using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class UserBenchmarkToggler : MonoBehaviour
    {
        const string SHORTCUT_TEXT = "P = Toggle Panel";

        public UserBenchmarkController userBenchmarkController;
        Text text;

        void Start()
        {
            text = GetComponent<Text>();
            text.text = SHORTCUT_TEXT;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.P))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            userBenchmarkController.gameObject.SetActive(!userBenchmarkController.gameObject.activeSelf);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }
    }
}