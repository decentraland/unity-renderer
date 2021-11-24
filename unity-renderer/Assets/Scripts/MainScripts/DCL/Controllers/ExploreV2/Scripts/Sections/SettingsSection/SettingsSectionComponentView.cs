using DCL;
using UnityEngine;

public interface ISettingsSectionComponentView
{
    /// <summary>
    /// Encapsulates the settings panel HUD into the section.
    /// </summary>
    void ConfigureSettings();
}

public class SettingsSectionComponentView : BaseComponentView, ISettingsSectionComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Transform contentContainer;

    public override void RefreshControl() { }

    public void ConfigureSettings() { DataStore.i.exploreV2.configureSettingsInFullscreenMenu.Set(contentContainer, true); }
}