using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.WebPlugin;
using System;

namespace DCL.HUD.Common
{
    /// <summary>
    /// Attaching this component to a scroll rect to apply the scroll sensitivity based on the os based stored sensitivities
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectSensitivityHandler : MonoBehaviour
    {
        private const float WINDOWS_SENSITIVITY_MULTIPLIER = 15f;
        private const float MAC_SENSITIVITY_MULTIPLIER = 4f;
        private const float LINUX_SENSITIVITY_MULTIPLIER = 3f;
        private const float DEFAULT_SENSITIVITY_MULTIPLIER = 3.5f;

        private ScrollRect myScrollRect;
        private float defaultSens;

        private void Awake()
        {
            myScrollRect = GetComponent<ScrollRect>();
            defaultSens = myScrollRect.scrollSensitivity;
            SetScrollRectSensitivity();
        }

        private void SetScrollRectSensitivity()
        {
            float scrollSensitivity = defaultSens * GetScrollMultiplier();
            Debug.Log($"ScrollRectSens.scrollSensitivity: {scrollSensitivity}, root: {transform.root}, go: {gameObject.name}");
            myScrollRect.scrollSensitivity = scrollSensitivity;
        }

        private float GetScrollMultiplier()
        {
            OperatingSystemFamily currentOperatingSystem = GetCurrentOperatingSystem();

            Debug.Log($"ScrollRectSens.GetCurrentOperatingSystem: {currentOperatingSystem}");

            switch (currentOperatingSystem)
            {
                case OperatingSystemFamily.Windows:
                    return WINDOWS_SENSITIVITY_MULTIPLIER;
                case OperatingSystemFamily.Linux:
                    return LINUX_SENSITIVITY_MULTIPLIER;
                case OperatingSystemFamily.MacOSX:
                    return MAC_SENSITIVITY_MULTIPLIER;
                default:
                    return DEFAULT_SENSITIVITY_MULTIPLIER;
            }
        }

        private OperatingSystemFamily GetCurrentOperatingSystem() {
#if UNITY_WEBGL
        return ObtainOsFromWebGLAgent();
#else
            return SystemInfo.operatingSystemFamily;
#endif
        }

        private OperatingSystemFamily ObtainOsFromWebGLAgent() {
            String agentInfo = WebGLPlugin.GetUserAgent();
            if (agentInfo.ToLower().Contains("windows"))
            {
                return OperatingSystemFamily.Windows;
            }
            else if (agentInfo.ToLower().Contains("mac") || agentInfo.ToLower().Contains("osx") || agentInfo.ToLower().Contains("os x"))
            {
                return OperatingSystemFamily.MacOSX;
            }
            else if (agentInfo.ToLower().Contains("linux"))
            {
                return OperatingSystemFamily.Linux;
            }
            else
            {
                return OperatingSystemFamily.Other;
            }
        }

    }
}
