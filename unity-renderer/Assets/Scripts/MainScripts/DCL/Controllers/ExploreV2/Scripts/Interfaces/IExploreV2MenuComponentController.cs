using System;

public enum ExploreSection
{
    Explore = 0,
    Quest = 1,
    Backpack = 2,
    Map = 3,
    Settings = 4,
    Wallet = 5,
    MyAccount = 6,
}

public interface IExploreV2MenuComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}
