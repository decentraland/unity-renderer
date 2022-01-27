using DCL;
using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.WebPlugin;

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
        //Debug.Log($"User agent is: {WebGLPlugin.GetUserAgent()}");
    }

    private void SetScrollSensBasedOnOS() {
        switch (SystemInfo.operatingSystemFamily)
        {
            case OperatingSystemFamily.Windows:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityWindows.Get();
                break;
            case OperatingSystemFamily.Linux:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityLinux.Get();
                break;
            case OperatingSystemFamily.MacOSX:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityMac.Get();
                break;
            default:
                myScrollRect.scrollSensitivity = DataStore.i.HUDs.scrollSensitivityGeneric.Get();
                break;
        }
    }

}
