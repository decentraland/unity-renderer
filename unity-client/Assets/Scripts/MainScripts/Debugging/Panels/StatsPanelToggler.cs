using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class StatsPanelToggler : MonoBehaviour
    {
        const string SHORTCUT_TEXT =
            "P = Toggle Panel | R = Reset Profile Data | I = Toggle Deep Msg Profiling";

        bool deepProfilingEnabled;
        Text text;

        public MessageBenchmarkController messageController;
        public MiscBenchmarkController miscController;

        void Start()
        {
            text = GetComponent<Text>();
            deepProfilingEnabled = true;

            EnableProfiling();
            RefreshShortcutText();
        }

        void Update()
        {
            UpdateDeepProfilingInput();

            if (Input.GetKeyUp(KeyCode.P))
            {
                TogglePanel();
            }
        }

        private void UpdateDeepProfilingInput()
        {
            if (Input.GetKeyUp(KeyCode.I))
            {
                deepProfilingEnabled = !deepProfilingEnabled;

                if (deepProfilingEnabled)
                {
                    EnableDeepProfiling();
                }
                else
                {
                    DisableDeepProfiling();
                }

                RefreshShortcutText();
            }
        }

        private void TogglePanel()
        {
            if (deepProfilingEnabled)
            {
                messageController.gameObject.SetActive(!messageController.gameObject.activeSelf);
            }

            miscController.gameObject.SetActive(!miscController.gameObject.activeSelf);
            Utils.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }

        private void EnableDeepProfiling()
        {
            messageController.StartProfiling();

            if (miscController.gameObject.activeSelf)
            {
                messageController.gameObject.SetActive(true);
            }
        }

        private void DisableDeepProfiling()
        {
            messageController.StopProfiling();
            messageController.gameObject.SetActive(false);
        }

        private void EnableProfiling()
        {
            miscController.StartProfiling();

            if (deepProfilingEnabled)
            {
                EnableDeepProfiling();
            }
        }

        void RefreshShortcutText()
        {
            if (deepProfilingEnabled)
            {
                text.text = $"[Deep Message Profiling Enabled]\n{SHORTCUT_TEXT}";
                return;
            }

            text.text = $"[Profiling Enabled]\n{SHORTCUT_TEXT}";
        }
    }
}