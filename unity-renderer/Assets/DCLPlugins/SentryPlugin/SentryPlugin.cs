using DCL;
using Sentry.Extensibility;

namespace DCLPlugins.SentryPlugin
{
    public class SentryPlugin : IPlugin
    {
        private readonly SentryController controller;
        public SentryPlugin()
        {
            var sentryHub = DisabledHub.Instance;
            controller = new SentryController(DataStore.i.player, DataStore.i.realm, sentryHub);
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}
