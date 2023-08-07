using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NFTIconComponentView : BaseComponentView, INFTIconComponentView, IComponentModelConfig<NFTIconComponentModel>
{
    private const string NAME_TYPE = "name";
    private const string PARCEL_TYPE = "parcel";
    private const string ESTATE_TYPE = "estate";

    [SerializeField] internal ButtonComponentView marketplaceButton;
    [SerializeField] internal Button detailInfoButton;
    [SerializeField] internal TMP_Text nftName;
    [SerializeField] internal TMP_Text nftNameMarketPlace;
    [SerializeField] internal GameObject marketplaceSection;
    [SerializeField] internal GameObject outline;
    [SerializeField] internal ImageComponentView nftImage;
    [SerializeField] internal TMP_Text nftTextInsteadOfImage;
    [SerializeField] internal ImageComponentView typeImage;
    [SerializeField] internal Image backgroundImage;
    [SerializeField] internal Image backgroundImageGradient;
    [SerializeField] internal Image rarityBackgroundImage;
    [SerializeField] internal NFTTypeIconsAndColors nftTypesIcons;
    [SerializeField] internal NFTSkinFactory skinFactory;
    [SerializeField] internal SizeOnHover sizeOnHoverComponent;
    [SerializeField] internal Transform nftItemInfoPositionRightRef;
    [SerializeField] internal Transform nftItemInfoPositionLeftRef;
    [SerializeField] internal Sprite normalBackground;
    [SerializeField] internal Sprite nameBackground;

    [SerializeField] internal NFTIconComponentModel model;

    public Button.ButtonClickedEvent onMarketplaceButtonClick => marketplaceButton.onClick;
    public Button.ButtonClickedEvent onDetailInfoButtonClick => detailInfoButton.onClick;

    private NFTItemInfo nftItemInfo;
    private NFTItemInfo.Model nftItemInfoCurrentModel;
    private bool showCategoryInfoOnNftItem;
    private string nftItemInfoRarity;
    private static readonly List<string> landTypes = new () { PARCEL_TYPE, ESTATE_TYPE };
    private static string nameType = NAME_TYPE;
    private static readonly Vector3 LAND_IMAGE_SCALE = new (5, 5, 5);
    private static readonly Vector3 NORMAL_IMAGE_SCALE = new (1, 1, 1);

    public void Configure(NFTIconComponentModel newModel)
    {
        if (model == newModel)
            return;

        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetName(model.name);
        SetShowMarketplaceButton(model.showMarketplaceButton);
        SetMarketplaceURI(model.marketplaceURI);
        SetType(model.type);
        SetImageURI(model.imageURI);
        SetShowType(model.showType);
        SetRarity(model.rarity);
    }

    public void SetName(string name)
    {
        model.name = name;

        if (nftName != null)
            nftName.text = name;

        if(nftNameMarketPlace != null)
            nftNameMarketPlace.text = name;
    }

    public void SetMarketplaceURI(string marketplaceURI)
    {
        model.marketplaceURI = marketplaceURI;
    }

    public void SetImageURI(string imageURI)
    {
        model.imageURI = imageURI;

        if (nftImage != null)
        {
            nftImage.gameObject.transform.localScale = landTypes.Contains(model.type) ? LAND_IMAGE_SCALE : NORMAL_IMAGE_SCALE;

            nftImage.SetImage(imageURI);
            nftImage.gameObject.SetActive(!string.IsNullOrEmpty(imageURI));
        }

        if (nftTextInsteadOfImage != null)
        {
            nftTextInsteadOfImage.text = model.name;
            nftTextInsteadOfImage.gameObject.SetActive(string.IsNullOrEmpty(imageURI));
        }
    }

    public void SetShowType(bool showType)
    {
        model.showType = showType;

        typeImage.gameObject.SetActive(showType);
        rarityBackgroundImage.gameObject.SetActive(showType);
    }

    public void SetType(string type)
    {
        model.type = type;
        typeImage.SetImage(nftTypesIcons.GetTypeImage(type));
    }

    public void SetRarity(string rarity)
    {
        model.rarity = rarity;
        Color rarityColor = nftTypesIcons.GetColor(rarity);

        backgroundImage.sprite = model.type == nameType ? nameBackground : normalBackground;
        backgroundImageGradient.enabled = model.type != nameType;
        backgroundImage.color = model.type == nameType ? Color.white : new Color(rarityColor.r, rarityColor.g, rarityColor.b, 1f);
        rarityBackgroundImage.color = rarityColor;
    }

    public void SetShowMarketplaceButton(bool showMarketplaceButton)
    {
        model.showMarketplaceButton = showMarketplaceButton;
    }

    public override void OnFocus()
    {
        if (sizeOnHoverComponent == null || !sizeOnHoverComponent.enabled)
            return;

        base.OnFocus();

        SetNFTIconAsSelected(true);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (nftItemInfo == null || !nftItemInfo.gameObject.activeSelf)
            SetNFTIconAsSelected(false);
    }

    public void ConfigureNFTItemInfo(NFTItemInfo nftItemInfoModal, WearableItem wearableItem, bool showCategoryInfo)
    {
        nftItemInfo = nftItemInfoModal;
        nftItemInfo.OnCloseButtonClick.RemoveAllListeners();
        nftItemInfo.OnCloseButtonClick.AddListener(() => SetNFTItemInfoActive(false));
        nftItemInfoCurrentModel = NFTItemInfo.Model.FromWearableItem(wearableItem);
        showCategoryInfoOnNftItem = showCategoryInfo;
        nftItemInfoRarity = wearableItem.rarity;
    }

    public void SetNFTItemInfoActive(bool isActive, bool showInLeftSide = false)
    {
        SetNFTIconAsSelected(isActive);
        sizeOnHoverComponent.enabled = !isActive;

        if (nftItemInfo != null)
        {
            nftItemInfo.SetActive(isActive);

            if (isActive)
            {
                nftItemInfo.SetModel(nftItemInfoCurrentModel);
                nftItemInfo.SetSkin(nftItemInfoRarity, skinFactory.GetSkinForRarity(nftItemInfoRarity));
                nftItemInfo.SetSellButtonActive(false);
                nftItemInfo.SetCategoryInfoActive(showCategoryInfoOnNftItem);
                nftItemInfo.transform.position = showInLeftSide ? nftItemInfoPositionLeftRef.position : nftItemInfoPositionRightRef.position;
            }
        }
    }

    private void SetNFTIconAsSelected(bool isSelected)
    {
        if (!model.showMarketplaceButton)
            return;

        marketplaceSection.SetActive(isSelected);
        outline.SetActive(isSelected);
    }
}
