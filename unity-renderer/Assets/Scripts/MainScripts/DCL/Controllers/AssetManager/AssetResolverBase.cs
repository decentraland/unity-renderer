using DCL;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainScripts.DCL.Controllers.AssetManager
{
    public abstract class AssetResolverBase
    {
        public const string VERBOSE_LOG_FLAG = "asset_resolver_verbose";

        protected DataStore_FeatureFlag featureFlags { get; }

        private bool IsVerboseAllowed() =>
            featureFlags.flags.Get().IsFeatureEnabled(VERBOSE_LOG_FLAG);

        protected static readonly AssetSource[] SOURCES = EnumUtils.Values<AssetSource>();

        protected AssetResolverBase(DataStore_FeatureFlag featureFlags)
        {
            this.featureFlags = featureFlags;
        }

        protected void LogVerbose(LogType logType, string message)
        {
            if (IsVerboseAllowed())
                Debug.unityLogger.Log(logType, message);
        }

        protected void LogVerbose(Exception e)
        {
            if (IsVerboseAllowed())
                Debug.LogException(e);
        }

        protected static bool HasFlag(AssetSource permittedSources, AssetSource source) =>
            EnumUtils.HasFlag(permittedSources, source);

        protected static PoolUtils.ListPoolRent<T> GetPermittedProviders<T>(IReadOnlyDictionary<AssetSource, T> providers, AssetSource permittedSources)
        {
            var listPoolRent = PoolUtils.RentList<T>();
            var permittedProviders = listPoolRent.GetList();

            foreach (AssetSource assetSource in SOURCES)
            {
                if (!HasFlag(permittedSources, assetSource))
                    continue;

                if (!providers.TryGetValue(assetSource, out var provider))
                    continue;

                permittedProviders.Add(provider);
            }

            return listPoolRent;
        }
    }
}
