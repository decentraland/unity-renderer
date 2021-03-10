public interface IPublishPopupController
{
    void Initialize(IPublishPopupView publishPopupView);
    void Dispose();
    void PublishStart();
    void PublishEnd(string message);
}

public class PublishPopupController : IPublishPopupController
{
    internal IPublishPopupView publishPopupView;

    public void Initialize(IPublishPopupView publishPopupView)
    {
        this.publishPopupView = publishPopupView;
    }

    public void Dispose()
    {
    }

    public void PublishStart()
    {
        publishPopupView.PublishStart();
    }

    public void PublishEnd(string message)
    {
        publishPopupView.PublishEnd(message);
    }
}
