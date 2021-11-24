using System;

/// <summary>
/// This interface is responsible to handle the lands of builder in world panel.
/// It will handle the listeners that will add/remove projects to the list   
/// </summary>
internal interface ILandsController
{
    /// <summary>
    /// This action will be called each time that we changes the land list
    /// </summary>
    event Action<LandWithAccess[]> OnLandsSet;

    /// <summary>
    /// This will set the land list
    /// </summary>
    /// <param name="lands"></param>
    void SetLands(LandWithAccess[] lands);

    /// <summary>
    /// This will add a listener that will be responsible to add/remove lands
    /// </summary>
    /// <param name="listener"></param>
    void AddListener(ILandsListener listener);

    /// <summary>
    /// This will remove a listener that will be responsible to add/remove lands 
    /// </summary>
    /// <param name="listener"></param>
    void RemoveListener(ILandsListener listener);

    /// <summary>
    /// This will return all the lands that we have access
    /// </summary>
    /// <returns>An array with the lands that we have access</returns>
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