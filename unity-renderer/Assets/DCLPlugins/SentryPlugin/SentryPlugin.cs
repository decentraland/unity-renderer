using DCL;
using DCLPlugins.SentryPlugin;
using Sentry;
using UnityEngine;
public class SentryPlugin : IPlugin
{
    private readonly SentryController controller;
    public SentryPlugin()
    {
        controller = new SentryController(DataStore.i.player, DataStore.i.realm);
    }

    public void Dispose()
    {
        controller?.Dispose();
    }
}
