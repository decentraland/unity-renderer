using System;

namespace MainScripts.DCL.Controllers.AssetManager
{
    /// <summary>
    ///     Possible sources an asset can come from.
    ///     Should be sorted by priority in an ascending order
    /// </summary>
    [Flags]
    public enum AssetSource
    {
        NONE = 0,

        /// <summary>
        ///     From the resources bundles at build time
        /// </summary>
        EMBEDDED = 1,

        /// <summary>
        ///     Downloaded over network
        /// </summary>
        WEB = 1 << 1,

        /// <summary>
        ///     Downloaded over Addressables
        /// </summary>
        ADDRESSABLE = 2 << 2,

        /// <summary>
        ///     All sources
        /// </summary>
        ALL = ~0,
    }
}
