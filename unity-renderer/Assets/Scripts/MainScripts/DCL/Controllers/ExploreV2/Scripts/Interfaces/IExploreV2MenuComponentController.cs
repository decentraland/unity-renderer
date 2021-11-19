using System;

public enum ExploreSection
{
    Explore = 0,
    Backpack = 1,
    Map = 2,
    Builder = 3,
    Quest = 4,
    Settings = 5
}

public interface IExploreV2MenuComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}