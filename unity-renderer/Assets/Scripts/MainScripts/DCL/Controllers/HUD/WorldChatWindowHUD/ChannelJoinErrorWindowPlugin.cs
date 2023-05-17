using DCL.Providers;
using System.Threading;

namespace DCL.Chat.HUD
{
    public class ChannelJoinErrorWindowPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private ChannelJoinErrorWindowController channelLimitReachedWindow;

        public ChannelJoinErrorWindowPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<ChannelJoinErrorWindowComponentView>("ChannelJoinErrorModal", cancellationToken: ct);

            channelLimitReachedWindow = new ChannelJoinErrorWindowController(view, Environment.i.serviceLocator.Get<IChatController>(), DataStore.i);
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            channelLimitReachedWindow.Dispose();
        }
    }
}
