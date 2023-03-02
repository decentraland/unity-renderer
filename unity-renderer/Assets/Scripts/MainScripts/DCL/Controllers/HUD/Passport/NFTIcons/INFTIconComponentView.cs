using UnityEngine.UI;

public interface INFTIconComponentView
{
    /// <summary>
    /// Event that will be triggered when the marketplace button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onMarketplaceButtonClick { get; }

    /// <summary>
    /// Event that will be triggered when the detail info button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onDetailInfoButtonClick { get; }

    /// <summary>
    /// Set the NFT marketplace uri.
    /// </summary>
    /// <param name="marketplaceURI">marketpace uri.</param>
    void SetMarketplaceURI(string marketplaceURI);

    /// <summary>
    /// Set the NFT image uri.
    /// </summary>
    /// <param name="imageURI">image uri.</param>
    void SetImageURI(string imageURI);

    /// <summary>
    /// Set the NFT name.
    /// </summary>
    /// <param name="nftName">name of the NFT.</param>
    void SetName(string nftName);

    /// <summary>
    /// Set the NFT type.
    /// </summary>
    /// <param name="type">type of the NFT. (LAND, Name, wearable type, emote)</param>
    void SetType(string type);

    /// <summary>
    /// Set the NFT rarity.
    /// </summary>
    /// <param name="rarity">rarity of the NFT.</param>
    void SetRarity(string rarity);

    /// <summary>
    /// Configure the NFT Item Info prompt for wearables.
    /// </summary>
    /// <param name="nftItemInfoModal"></param>
    /// <param name="wearable"></param>
    /// <param name="showCategoryInfo"></param>
    void ConfigureNFTItemInfo(NFTItemInfo nftItemInfoModal, WearableItem wearable, bool showCategoryInfo);

    /// <summary>
    /// Shows/Hides the NFT Item Info prompt.
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="showInLeftSide"></param>
    void SetNFTItemInfoActive(bool isActive, bool showInLeftSide = false);
}
