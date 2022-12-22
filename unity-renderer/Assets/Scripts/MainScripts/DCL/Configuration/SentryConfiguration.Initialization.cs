using Sentry.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Configuration
{
    // production == 'main' branch
    // development == 'dev' branch
    // branch/{branch} == 'branch/xyz'
    public static partial class SentryConfiguration
    {
        const string DEVELOPMENT = "Development";
        const string PRODUCTION = "production";
        const string UNKNOWN_BRANCH = "branch/unknown";
        public static void Initialize()
        {
            if (SentryConfiguration.Environment == UNKNOWN_BRANCH) return;

#if !UNITY_EDITOR
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
