using DCL;
using UnityEngine;

public interface IMapSectionComponentView
{
    /// <summary>
    /// Encapsulates the map HUD into the section.
    /// </summary>
    void ConfigureMap();
}

public class MapSectionComponentView : BaseComponentView, IMapSectionComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Transform contentContainer;

    public override void RefreshControl() { }

    public void ConfigureMap() { DataStore.i.exploreV2.configureMapInFullscreenMenu.Set(contentContainer, true); }
}