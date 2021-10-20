using System;

public interface IExploreV2MenuComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}