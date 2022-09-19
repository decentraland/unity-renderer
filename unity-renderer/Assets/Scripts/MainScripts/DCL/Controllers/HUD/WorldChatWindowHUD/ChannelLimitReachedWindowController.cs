namespace DCL.Chat.HUD
{
    public class ChannelLimitReachedWindowController : IHUD
    {
        private readonly IChannelLimitReachedWindowView view;

        public IChannelLimitReachedWindowView View => view;

        public ChannelLimitReachedWindowController(IChannelLimitReachedWindowView view)
        {
            this.view = view;
        }

        public void Dispose()
        {
            view.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                view.OnClose += HandleViewClosed;
                view.Show();
            }
            else
            {
                view.OnClose -= HandleViewClosed;
                view.Hide();
            }
        }

        private void HandleViewClosed() => SetVisibility(false);
    }
}