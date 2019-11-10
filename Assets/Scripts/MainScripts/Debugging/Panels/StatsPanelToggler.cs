using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class StatsPanelToggler : MonoBehaviour
    {
        const string SHORTCUT_TEXT =
            "P = Toggle Panel | R = Reset Profile Data | O = Toggle All Profiling | I = Toggle Deep Msg Profiling";

        bool allProfilingEnabled;
        bool deepProfilingEnabled;
        Text text;

        public MessageBenchmarkController messageController;
        public MiscBenchmarkController miscController;

        void Start()
        {
            text = GetComponent<Text>();
            allProfilingEnabled = false;
            deepProfilingEnabled = false;

            if (allProfilingEnabled)
                EnableProfiling();
            else
                DisableProfiling();

            RefreshShortcutText();
        }

        void Update()
        {
            UpdateAllProfilingInput();
            UpdateDeepProfilingInput();

            if (allProfilingEnabled)
            {
                if (Input.GetKeyUp(KeyCode.P))
                {
                    TogglePanel();
                }
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

        private void UpdateAllProfilingInput()
        {
            if (Input.GetKeyUp(KeyCode.O))
            {
                allProfilingEnabled = !allProfilingEnabled;

                if (allProfilingEnabled)
                {
                    EnableProfiling();
                }
                else
                {
                    DisableProfiling();
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
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
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

        private void DisableProfiling()
        {
            DisableDeepProfiling();
            miscController.StopProfiling();
            miscController.gameObject.SetActive(false);

            text.text = "";
        }

        void RefreshShortcutText()
        {
            if (allProfilingEnabled)
            {
                if (deepProfilingEnabled)
                {
                    text.text = $"[Deep Message Profiling Enabled]\n{SHORTCUT_TEXT}";
                }
                else
                {
                    text.text = $"[Profiling Enabled]\n{SHORTCUT_TEXT}";
                }
            }
            else
            {
                text.text = "";
            }
        }
    }
}
