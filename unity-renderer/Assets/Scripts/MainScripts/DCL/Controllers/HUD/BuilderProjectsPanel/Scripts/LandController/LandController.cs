using System;

internal class LandController
{
    public event Action<LandData[]> OnLandsSet;

    private LandData[] userLands = null;

    public void SetLands(LandData[] lands)
    {
        userLands = lands;
        OnLandsSet?.Invoke(lands);
    }

    public void AddListener(ILandsListener listener)
    {
        OnLandsSet += listener.OnSetLands;
        listener.OnSetLands(userLands);
    }

    public void RemoveListener(ILandsListener listener)
    {
        OnLandsSet -= listener.OnSetLands;
    }
}