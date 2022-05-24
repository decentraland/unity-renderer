using SocialFeaturesAnalytics;

public class VoiceChatWindowController : IHUD
{
    IVoiceChatWindowComponentView view;
    ISocialAnalytics socialAnalytics;

    public IVoiceChatWindowComponentView View => view;

    public VoiceChatWindowController(ISocialAnalytics socialAnalytics)
    {
        this.socialAnalytics = socialAnalytics;

        view = CreateView();
        view.Hide(instant: true);
        view.OnClose += OnViewClosed;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            view.Show();
        else
            view.Hide();
    }

    public void Dispose()
    {
        view.OnClose -= OnViewClosed;
    }

    internal void OnViewClosed() { SetVisibility(false); }

    protected internal virtual IVoiceChatWindowComponentView CreateView() => VoiceChatWindowComponentView.Create();
}
