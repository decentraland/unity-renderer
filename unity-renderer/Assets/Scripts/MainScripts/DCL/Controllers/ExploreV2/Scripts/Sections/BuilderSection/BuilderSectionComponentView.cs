using DCL;
using UnityEngine;

public interface IBuilderSectionComponentView
{
    /// <summary>
    /// Encapsulates the builder HUD into the section.
    /// </summary>
    void ConfigureBuilder();
}

public class BuilderSectionComponentView : BaseComponentView, IBuilderSectionComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Transform contentContainer;

    public override void RefreshControl() { }

    public void ConfigureBuilder() { DataStore.i.exploreV2.configureBuilderInFullscreenMenu.Set(contentContainer, true); }
}