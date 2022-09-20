using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationFocusService : IApplicationFocusService
{

    public event Action OnApplicationFocus;
    public event Action OnApplicationFocusLost;

    public void Initialize()
    {
        Application.focusChanged += FocusChange;
    }
    private void FocusChange(bool focus)
    {
        if (focus)
        {
            OnFocusGained();
        }
        else
        {
            OnFocusLost();
        }
    }

    internal void OnFocusGained()
    {
        OnApplicationFocus?.Invoke();
    }
    
    internal void OnFocusLost()
    {
        OnApplicationFocusLost?.Invoke();
    }
    
    public void Dispose()
    {
        Application.focusChanged -= FocusChange;
    }

}
