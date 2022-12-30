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
            // do nothing for now
        }
    }
}
