using DCL;
using DCLPlugins.SentryPlugin;
using Sentry;
using Sentry.Extensibility;
using UnityEngine;
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
