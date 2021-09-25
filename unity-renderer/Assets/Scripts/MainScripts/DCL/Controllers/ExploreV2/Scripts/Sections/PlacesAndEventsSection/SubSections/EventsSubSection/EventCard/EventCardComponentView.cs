public interface IEventCardComponentView
{
    /// <summary>
    /// Fill the model and updates the event card with this data.
    /// </summary>
    /// <param name="model">Data to configure the event card.</param>
    void Configure(EventCardComponentModel model);
}

public class EventCardComponentView : BaseComponentView, IEventCardComponentView
{
    public override void PostInitialization() { }

    public void Configure(EventCardComponentModel model) { }

    public override void RefreshControl() { }
}