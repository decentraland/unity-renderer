using DCL;
using Sentry;
using Sentry.Extensibility;
using System;
using System.Reflection;
using UnityEngine;

namespace DCLPlugins.SentryPlugin
{
    public class SentryPlugin : IPlugin
    {
        private readonly SentryController controller;

        public SentryPlugin()
        {
            // Sentry doesn't provide a public getter accessor to its instance
            // So we unfortunately need a bit of magic
            Type type = typeof(SentrySdk);
            FieldInfo info = type.GetField("CurrentHub", BindingFlags.NonPublic | BindingFlags.Static);
            if (info == null)
            {
                Debug.LogError("SentrySdk field could not be reflected. Sentry will not be initialized.");
                return;
            }

            controller = new SentryController(DataStore.i.player, DataStore.i.realm, info.GetValue(null) as IHub);
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}
