using System;

public interface IHighlightsSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;
}

public class HighlightsSubSectionComponentController : IHighlightsSubSectionComponentController
{
    public event Action OnCloseExploreV2;

    public void Dispose() { }
}