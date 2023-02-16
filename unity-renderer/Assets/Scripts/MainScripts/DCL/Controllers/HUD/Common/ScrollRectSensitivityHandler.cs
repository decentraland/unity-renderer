using MainScripts.DCL.WebPlugin;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.HUD.Common
{
    /// <summary>
    /// Attaching this component to a scroll rect to apply the scroll sensitivity based on the os based stored sensitivities
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectSensitivityHandler : MonoBehaviour
    {
        private const float WINDOWS_SENSITIVITY_MULTIPLIER = 4.5f;
        private const float MAC_SENSITIVITY_MULTIPLIER = 1.5f;
        private const float LINUX_SENSITIVITY_MULTIPLIER = 1.125f;
        private const float DEFAULT_SENSITIVITY_MULTIPLIER = 1.32f;

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
            myScrollRect.scrollSensitivity = scrollSensitivity;
        }

        private float GetScrollMultiplier()
        {
            OperatingSystemFamily currentOperatingSystem = GetCurrentOperatingSystem();

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
            string agentInfo = WebGLPlugin.GetUserAgent();

            if (agentInfo.Contains("windows", StringComparison.OrdinalIgnoreCase))
                return OperatingSystemFamily.Windows;

            if (agentInfo.Contains("mac", StringComparison.OrdinalIgnoreCase)
                || agentInfo.Contains("osx", StringComparison.OrdinalIgnoreCase)
                || agentInfo.Contains("os x", StringComparison.OrdinalIgnoreCase))
                return OperatingSystemFamily.MacOSX;

            if (agentInfo.Contains("linux", StringComparison.OrdinalIgnoreCase))
                return OperatingSystemFamily.Linux;

            return OperatingSystemFamily.Other;
        }

    }
}
