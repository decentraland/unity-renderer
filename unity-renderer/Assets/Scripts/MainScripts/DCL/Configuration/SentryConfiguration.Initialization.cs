using Sentry;
using Sentry.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Configuration
{
    public static partial class SentryConfiguration
    {
        // production == 'main' branch
        // development == 'dev' branch
        // branch/{branch} == 'branch/xyz'
        const string DEVELOPMENT = "development";
        const string PRODUCTION = "production";
        const string UNKNOWN_BRANCH = "branch/unknown";
        public static void Initialize()
        {
            // if (SentryConfiguration.Environment == UNKNOWN_BRANCH) return;
            SentrySdk.CaptureMessage("Sentry is initializng...test 1");
#if !UNITY_EDITOR
            SentrySdk.CaptureMessage("Sentry is initializng...test 2");
            SentryUnity.Init(o =>
            {
                o.Environment = SentryConfiguration.Environment;
                o.Dsn = SentryConfiguration.Dsn;
                o.Release = SentryConfiguration.Release;

                // Enable performance trace sampling on development builds only
                if (SentryConfiguration.Environment == DEVELOPMENT)
                {
                    o.TracesSampleRate = 1.0f;
                }
            });
#endif
        }
    }
}
