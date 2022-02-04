using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.WebPlugin;
using System;

/// <summary>
/// Attaching this component to a scroll rect to apply the scroll sensitivity based on the os based stored sensitivities
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectSensitivityHandler : MonoBehaviour
{

    private const float windowsSensitivityMultiplier = 2.7f;
    private const float macSensitivityMultiplier = 3f;
    private const float linuxSensitivityMultiplier = 2.5f;
    private const float defaultSensitivityMultiplier = 2f;

    private ScrollRect myScrollRect;
    private float defaultSens;

    void Awake()
    {
        myScrollRect = GetComponent<ScrollRect>();
        defaultSens = myScrollRect.scrollSensitivity;
        SetScrollRectSensitivity();
    }

    private void SetScrollRectSensitivity() 
    {
        myScrollRect.scrollSensitivity = defaultSens * GetScrollMultiplier();
    }

    private float GetScrollMultiplier() {
        switch (GetCurrentOs())
        {
            case OperatingSystemFamily.Windows:
                return windowsSensitivityMultiplier;
            case OperatingSystemFamily.Linux:
                return linuxSensitivityMultiplier;
            case OperatingSystemFamily.MacOSX:
                return macSensitivityMultiplier;
            default:
                return defaultSensitivityMultiplier;
        }
    }

    private OperatingSystemFamily GetCurrentOs() {
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
