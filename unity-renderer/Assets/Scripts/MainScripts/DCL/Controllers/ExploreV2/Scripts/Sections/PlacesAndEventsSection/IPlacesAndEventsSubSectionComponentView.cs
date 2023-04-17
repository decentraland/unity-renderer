public interface IPlacesAndEventsSubSectionComponentView
{
    /// <summary>
    /// Set the current scroll view position to 1.
    /// </summary>
    void RestartScrollViewPosition();

    void SetAllAsLoading();

    int CurrentTilesPerRow { get; }
    int CurrentGoingTilesPerRow { get; }
}
