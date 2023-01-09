public interface IPlacesAndEventsSubSectionComponentView
{
    void RestartScrollViewPosition();

    void SetAllAsLoading();

    int CurrentTilesPerRow { get; }
}
