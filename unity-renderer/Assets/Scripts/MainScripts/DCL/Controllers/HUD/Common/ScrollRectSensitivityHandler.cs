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

    private ScrollRect myScrollRect;

    void Awake()
    {
        myScrollRect = GetComponent<ScrollRect>();
        SetScrollSensBasedOnOS();
        //Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36
        Debug.Log($"User agent is: {WebGLPlugin.GetUserAgent()}");
    }

    private void SetScrollSensBasedOnOS() {
        switch (GetCurrentOs())
        {
            case OS_SYSTEM.windows:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityWindows.Get();
                break;
            case OS_SYSTEM.linux:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityLinux.Get();
                break;
            case OS_SYSTEM.macosx:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityMac.Get();
                break;
            default:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityGeneric.Get();
                break;
        }
    }

    private OS_SYSTEM GetCurrentOs() {
#if UNITY_WEBGL
        return ObtainOsFromWebGLAgent();
#else
        return (OS_SYSTEM) Enum.Parse(typeof(OS_SYSTEM), SystemInfo.operatingSystemFamily.ToString());
#endif
    }

    private OS_SYSTEM ObtainOsFromWebGLAgent() {
        String agentInfo = WebGLPlugin.GetUserAgent();
        if (agentInfo.ToLower().Contains("windows"))
        {
            Debug.Log("OS IS WINDOWS");
            return OS_SYSTEM.windows;
        }
        else if (agentInfo.ToLower().Contains("mac") || agentInfo.ToLower().Contains("osx") || agentInfo.ToLower().Contains("os x"))
        {
            Debug.Log("OS IS MAC");
            return OS_SYSTEM.macosx;
        }
        else if (agentInfo.ToLower().Contains("linux"))
        {
            Debug.Log("OS IS LINUX");
            return OS_SYSTEM.linux;
        }
        else
        {
            Debug.Log("OS IS OTHER");
            return OS_SYSTEM.other;
        }
    }

    private enum OS_SYSTEM { 
        windows,
        linux,
        macosx,
        other
    }

}
