using System;

internal interface ILandController
{ 
    event Action<LandWithAccess[]> OnLandsSet;
    void SetLands(LandWithAccess[] lands);
    void AddListener(ILandsListener listener);
    void RemoveListener(ILandsListener listener);
}

internal class LandController : ILandController
{
    public event Action<LandWithAccess[]> OnLandsSet;

    private LandWithAccess[] userLands = null;

    void ILandController.SetLands(LandWithAccess[] lands)
    {
        userLands = lands;
        OnLandsSet?.Invoke(lands);
    }

    void ILandController.AddListener(ILandsListener listener)
    {
        OnLandsSet += listener.OnSetLands;
        listener.OnSetLands(userLands);
    }

    void ILandController.RemoveListener(ILandsListener listener)
    {
        OnLandsSet -= listener.OnSetLands;
    }
}