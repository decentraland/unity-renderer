public interface IPublishPopupController
{
    public float currentProgress { get; }

    void Initialize(IPublishPopupView publishPopupView);
    void Dispose();
    void PublishStart();
    void PublishEnd(bool isOk, string message);
    void SetPercentage(float newValue);
}

public class PublishPopupController : IPublishPopupController
{
    public float currentProgress => publishPopupView.currentProgress;

    internal IPublishPopupView publishPopupView;

    public void Initialize(IPublishPopupView publishPopupView) { this.publishPopupView = publishPopupView; }

    public void Dispose() { }

    public void PublishStart()
    {
        publishPopupView.PublishStart();
        SetPercentage(0f);
    }

    public void PublishEnd(bool isOk, string message) { publishPopupView.PublishEnd(isOk, message); }

    public void SetPercentage(float newValue) { publishPopupView.SetPercentage(newValue); }
}