using System;

[Serializable]
public class NFTIconComponentModel : BaseComponentModel
{
    public bool showType;
    public bool showMarketplaceButton;
    public string type;
    public string marketplaceURI;
    public string name;
    public string rarity;
    public string imageURI;
    public NftInfo nftInfo;
}
