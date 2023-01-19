using DCL;
using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.AssetManager
{
    public static class AssetResolverLogger
    {
        public const string VERBOSE_LOG_FLAG = "asset_resolver_verbose";

        private static bool IsVerboseAllowed(this DataStore_FeatureFlag featureFlags) =>
            featureFlags.flags.Get().IsFeatureEnabled(VERBOSE_LOG_FLAG);

        public static void LogVerbose(DataStore_FeatureFlag featureFlags, LogType logType, string message)
        {
            if (IsVerboseAllowed(featureFlags))
                Debug.unityLogger.Log(logType, message);
        }

        public static void LogVerbose(DataStore_FeatureFlag featureFlags, Exception e)
        {
            if (IsVerboseAllowed(featureFlags))
                Debug.LogException(e);
        }
    }
}
