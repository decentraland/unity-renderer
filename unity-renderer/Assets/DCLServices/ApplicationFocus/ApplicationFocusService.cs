using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationFocusService : IApplicationFocusService
{

    public event Action<bool> OnApplicationFocus;
    private bool currentFocusState;

    public void Initialize()
    {
        Application.focusChanged += FocusChange;
        currentFocusState = Application.isFocused;
    }
    private void FocusChange(bool focus)
    {
        if (currentFocusState == focus)
            return;

        if (focus)
            OnFocusGained();
        else
            OnFocusLost();

        currentFocusState = focus;
    }

    internal void OnFocusGained() =>
        OnApplicationFocus?.Invoke(true);

    internal void OnFocusLost() =>
        OnApplicationFocus?.Invoke(false);

    public void Dispose() =>
        Application.focusChanged -= FocusChange;

    public bool IsApplicationFocused() => currentFocusState;

}