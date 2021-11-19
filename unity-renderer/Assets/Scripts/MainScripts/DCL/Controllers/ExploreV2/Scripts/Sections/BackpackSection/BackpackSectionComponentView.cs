using DCL;
using UnityEngine;

public interface IBackpackSectionComponentView
{
    /// <summary>
    /// Encapsulates the backpack HUD into the section.
    /// </summary>
    void ConfigureBackpack();
}

public class BackpackSectionComponentView : BaseComponentView, IBackpackSectionComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Transform contentContainer;

    public override void RefreshControl() { ConfigureBackpack(); }

    public void ConfigureBackpack() { DataStore.i.exploreV2.showBackpackInMenuMode.Set(contentContainer, true); }
}