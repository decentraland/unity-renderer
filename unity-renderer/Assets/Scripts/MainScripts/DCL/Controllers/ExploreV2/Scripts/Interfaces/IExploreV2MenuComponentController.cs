using System;

public enum ExploreSection
{
    Explore = 0,
    Quest = 1,
    Backpack = 2,
    Map = 3,
    Builder = 4,
    Settings = 5
}

public interface IExploreV2MenuComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}