using System;

/// <summary>
/// This is the main template for the features handled by PluginSystem
/// </summary>
public interface IPlugin : IDisposable
{
    void Initialize();
    void OnGUI();
    void Update();
    void LateUpdate();
}