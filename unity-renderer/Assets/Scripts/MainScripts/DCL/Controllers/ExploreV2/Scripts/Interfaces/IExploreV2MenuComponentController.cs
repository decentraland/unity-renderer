using System;

public enum ExploreSection
{
    Explore = 0,
    Quest = 1,
    Backpack = 2,
    CameraReel = 3,
    Map = 4,
    Settings = 5,
    Wallet = 6,
    MyAccount = 7,
}

public interface IExploreV2MenuComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}
