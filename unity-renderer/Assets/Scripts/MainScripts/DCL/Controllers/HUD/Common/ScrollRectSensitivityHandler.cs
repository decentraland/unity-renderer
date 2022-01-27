using DCL;
using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.WebPlugin;
using System;
using System.IO;
using UnityEngine.Networking;

/// <summary>
/// Attaching this component to a scroll rect to apply the scroll sensitivity based on the os based stored sensitivities
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectSensitivityHandler : MonoBehaviour
{

    private const float windowsSensitivityMultiplier = 1f;
    private const float macSensitivityMultiplier = 3f;
    private const float linuxSensitivityMultiplier = 2.5f;
    private const float defaultSensitivityMultiplier = 1.5f;

    private ScrollRect myScrollRect;
    private float defaultSens;
    private float tempSens = 3;

    void Awake()
    {
        myScrollRect = GetComponent<ScrollRect>();
        defaultSens = myScrollRect.scrollSensitivity;
        SetScrollRectSensitivity();
        //Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36
        Debug.Log($"User agent is: {WebGLPlugin.GetUserAgent()}");
    }

    private void SetScrollRectSensitivity() 
    {
        myScrollRect.scrollSensitivity *= GetScrollMultiplier();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            tempSens += 1;
            myScrollRect.scrollSensitivity = defaultSens*tempSens;
            Debug.Log($"Current sens is {tempSens}");
        }
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            tempSens -= 1;
            myScrollRect.scrollSensitivity = defaultSens * tempSens;
            Debug.Log($"Current sens is {tempSens}");
        }
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
            Debug.Log("OS IS WINDOWS");
            return OperatingSystemFamily.Windows;
        }
        else if (agentInfo.ToLower().Contains("mac") || agentInfo.ToLower().Contains("osx") || agentInfo.ToLower().Contains("os x"))
        {
            Debug.Log("OS IS MAC");
            return OperatingSystemFamily.MacOSX;
        }
        else if (agentInfo.ToLower().Contains("linux"))
        {
            Debug.Log("OS IS LINUX");
            return OperatingSystemFamily.Linux;
        }
        else
        {
            Debug.Log("OS IS OTHER");
            return OperatingSystemFamily.Other;
        }
    }

}
