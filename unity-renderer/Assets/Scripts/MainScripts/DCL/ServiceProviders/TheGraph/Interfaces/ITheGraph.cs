using System;
using System.Collections.Generic;
using DCL.Helpers;

public enum TheGraphCache
{
    DontUseCache,
    UseCache
}

public enum TheGraphNetwork
{
    Ethereum,
    Polygon,
}

public interface ITheGraph : IDisposable
{
    Promise<List<Land>> QueryLands(string network, string address, float cacheMaxAgeSeconds);
    Promise<double> QueryMana(string address, TheGraphNetwork network);

    /// <summary>
    /// Get the list of NFTs owned by an user.
    /// </summary>
    /// <param name="address">User Id</param>
    /// <param name="layer">ETHEREUM or MATIC</param>
    /// <returns>A list of NFTs (limited to 100).</returns>
    Promise<List<Nft>> QueryNftCollections(string address, NftCollectionsLayer layer);

    /// <summary>
    /// Get the list of NFTs owned by an user and filtered by urn.
    /// </summary>
    /// <param name="address">User Id</param>
    /// <param name="urn">NFT's urn</param>
    /// <param name="layer">ETHEREUM or MATIC</param>
    /// <returns>A list of NFTs (limited to 100).</returns>
    Promise<List<Nft>> QueryNftCollectionsByUrn(string address, string urn, NftCollectionsLayer layer);
}
