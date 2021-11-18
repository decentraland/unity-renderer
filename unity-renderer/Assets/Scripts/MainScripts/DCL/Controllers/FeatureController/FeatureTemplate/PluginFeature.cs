using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the main template for the features handled by FeatureController
///
/// This will handle all the monobehaviour callbacks when your feature is activated
/// </summary>
public abstract class PluginFeature
{
    public virtual void Initialize() { }

    //Note: Do not use OnGUI, try to avoid it if possible since it can be called multiple times per frame
    public virtual void OnGUI() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void Dispose() { }
}

public interface IPlugin : IDisposable
{
    bool enabled { get; }
    void Enable();
    void Disable();
    void OnGUI();
    void Update();
    void LateUpdate();
}