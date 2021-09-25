using UnityEngine;

public interface IEventsSubSectionComponentView
{
    /// <summary>
    /// Fill the model and updates the Events sub-section with this data.
    /// </summary>
    /// <param name="model">Data to configure the Events sub-section.</param>
    void Configure(EventsSubSectionComponentModel model);

    /// <summary>
    /// Set the featured events.
    /// </summary>
    /// <param name="featuredEventsInfo">Featured events model (carousel).</param>
    void SetFeaturedEvents(CarouselComponentModel featuredEventsInfo);

    /// <summary>
    /// Set the upcoming events.
    /// </summary>
    /// <param name="upcomingEventsInfo">Upcoming events model (carousel).</param>
    void SetUpcomingEvents(GridContainerComponentModel upcomingEventsInfo);
}

public class EventsSubSectionComponentView : BaseComponentView, IEventsSubSectionComponentView
{
    [SerializeField] internal CarouselComponentView featuredEvents;
    [SerializeField] internal GridContainerComponentView upcomingEvents;

    [Header("Configuration")]
    [SerializeField] internal EventsSubSectionComponentModel model;

    public override void PostInitialization() { Configure(model); }

    public void Configure(EventsSubSectionComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetFeaturedEvents(model.featuredEvents);
        SetUpcomingEvents(model.upcomingEvents);
    }

    public void SetFeaturedEvents(CarouselComponentModel featuredEventsInfo)
    {
        model.featuredEvents = featuredEventsInfo;

        if (featuredEvents == null)
            return;

        featuredEvents.Configure(featuredEventsInfo);
    }

    public void SetUpcomingEvents(GridContainerComponentModel upcomingEventsInfo)
    {
        model.upcomingEvents = upcomingEventsInfo;

        if (upcomingEvents == null)
            return;

        upcomingEvents.Configure(upcomingEventsInfo);
    }
}