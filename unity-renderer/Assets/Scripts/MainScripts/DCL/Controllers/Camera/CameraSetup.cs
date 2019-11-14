using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("CameraTests")]
public abstract class CameraSetup
{
    public abstract void Activate();
    public abstract void Deactivate();
}

public abstract class CameraSetup<TConfig> : CameraSetup, IDisposable
{
    internal Transform cameraTransform;
    internal BaseVariable<TConfig> configuration;

    public CameraSetup(Transform cameraTransform, BaseVariable<TConfig> configuration)
    {
        this.cameraTransform = cameraTransform;
        this.configuration = configuration;
    }

    public sealed override void Activate()
    {
        configuration.OnChange += OnConfigChanged;
        SetUp();
    }

    public sealed  override void Deactivate()
    {
        configuration.OnChange -= OnConfigChanged;
    }

    protected abstract void SetUp();
    protected abstract void OnConfigChanged(TConfig newConfig, TConfig oldConfig);

    public virtual void Dispose()
    {
        configuration.OnChange -= OnConfigChanged;
        CleanUp();
    }
    
    protected abstract void CleanUp();
}