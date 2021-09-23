using System;

public interface IExploreV2MenuComponentController
{
    event Action OnOpen;
    event Action OnClose;

    void Initialize();
    void Dispose();
    void SetVisibility(bool visible);
}