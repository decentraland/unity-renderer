using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusStateBridge : MonoBehaviour
{
    public void Awake()
    {
        CommonScriptableObjects.focusState.Set(true);
    }

    public void ReportFocusOn()
    {
        CommonScriptableObjects.focusState.Set(true);
    }

    public void ReportFocusOff()
    {
        CommonScriptableObjects.focusState.Set(false);
    }
}