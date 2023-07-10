using System;

public enum NftCollectionsLayer
{
    ETHEREUM,
    MATIC
}

public class Nft
{
    public string collectionId;
    public string tokenId;
    public string urn;
    public string blockchainId;
}

[Serializable]
internal class NftQueryResultWrapped
{
    public NftQueryResult data;
}

[Serializable]
internal class NftQueryResult
{
    public NftData[] nfts;
}

[Serializable]
internal class NftData
{
    public NftCollectionData collection;
    public NftItemData item;
    public string tokenId;
    public string urn;
}

[Serializable]
internal class NftCollectionData
{
    public string id;
}

[Serializable]
internal class NftItemData
{
    public string blockchainId;
}
