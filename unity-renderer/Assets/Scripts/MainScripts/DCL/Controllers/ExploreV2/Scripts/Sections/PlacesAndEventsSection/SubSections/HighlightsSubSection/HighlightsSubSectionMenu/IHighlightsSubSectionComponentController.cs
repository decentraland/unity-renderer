// unset:none
using System;

public interface IHighlightsSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// It will be triggered when the sub-section want to request to go to the Events sub-section.
    /// </summary>
    event Action OnGoToEventsSubSection;
}
