/// <summary>
/// This interface handle the set of the lands 
/// </summary>
internal interface ILandsListener
{
    /// <summary>
    /// This will set the lands in the inheritor
    /// </summary>
    /// <param name="lands">lands to set</param>
    void OnSetLands(LandWithAccess[] lands);
}