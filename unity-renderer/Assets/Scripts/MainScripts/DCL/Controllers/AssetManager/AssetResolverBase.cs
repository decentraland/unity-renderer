using DCL;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;

namespace MainScripts.DCL.Controllers.AssetManager
{
    public abstract class AssetResolverBase
    {
        protected static readonly AssetSource[] SOURCES = EnumUtils.Values<AssetSource>();

        protected DataStore_FeatureFlag featureFlags { get; }

        protected AssetResolverBase(DataStore_FeatureFlag featureFlags)
        {
            this.featureFlags = featureFlags;
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
