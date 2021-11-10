using UnityEngine;

public interface IHighlightsSubSectionComponentView { }

public class HighlightsSubSectionComponentView : BaseComponentView, IHighlightsSubSectionComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal CarouselComponentView promotedPlaces;
    [SerializeField] internal GridContainerComponentView featuredPlaces;
    [SerializeField] internal GridContainerComponentView liveEvents;

    public override void RefreshControl()
    {
        promotedPlaces.RefreshControl();
        featuredPlaces.RefreshControl();
        liveEvents.RefreshControl();
    }
}