using System;

internal interface ILandsController
{
    event Action<LandWithAccess[]> OnLandsSet;
    void SetLands(LandWithAccess[] lands);
    void AddListener(ILandsListener listener);
    void RemoveListener(ILandsListener listener);
    LandWithAccess[] GetLands();
}

internal class LandsController : ILandsController
{
    public event Action<LandWithAccess[]> OnLandsSet;

    private LandWithAccess[] userLands = null;

    void ILandsController.SetLands(LandWithAccess[] lands)
    {
        userLands = lands;
        OnLandsSet?.Invoke(lands);
    }

    void ILandsController.AddListener(ILandsListener listener)
    {
        OnLandsSet += listener.OnSetLands;
        listener.OnSetLands(userLands);
    }

    void ILandsController.RemoveListener(ILandsListener listener) { OnLandsSet -= listener.OnSetLands; }
    public LandWithAccess[] GetLands() { return userLands; }
}