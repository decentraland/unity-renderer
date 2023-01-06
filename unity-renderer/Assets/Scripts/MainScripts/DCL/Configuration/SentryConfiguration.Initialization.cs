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
        private const string DEVELOPMENT = "development";
        private const string PRODUCTION = "production";
        private const string UNKNOWN_BRANCH = "branch/unknown";

        public static void Initialize()
        {
            if (SentryConfiguration.Environment == UNKNOWN_BRANCH) return;

            if (EnvironmentSettings.RUNNING_TESTS) return;

            #if !UNITY_EDITOR
            SentryUnity.Init(o =>
            {
                o.Environment = SentryConfiguration.Environment;
                o.Dsn = SentryConfiguration.Dsn;
                o.Release = SentryConfiguration.Release;
                o.StackTraceMode = StackTraceMode.Enhanced;
                o.EnableLogDebouncing = false;

                // Enable performance trace sampling on development builds only
                if (SentryConfiguration.Environment == DEVELOPMENT)
                {
                    o.TracesSampleRate = 1.0f;
                }
            });

            Debug.LogError($"DSN: {SentryConfiguration.Dsn} ENV: {SentryConfiguration.Environment} RELEASE: {SentryConfiguration.Release}");
            SentrySdk.CaptureMessage("Sentry is initialized. ");
            #endif
        }
    }
}
