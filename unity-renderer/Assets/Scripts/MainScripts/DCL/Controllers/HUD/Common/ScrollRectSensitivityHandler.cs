using DCL;
using UnityEngine;
using UnityEngine.UI;

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
