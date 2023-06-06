using System;

public enum ExploreSection
{
    Explore = 0,
    Backpack = 1,
    Map = 2,
    Quest = 3,
    Settings = 4,
    Wallet = 5,
}

public interface IExploreV2MenuComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}
