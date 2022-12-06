using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class NFTIconComponentView : BaseComponentView, INFTIconComponentView, IComponentModelConfig<NFTIconComponentModel>
{

    [SerializeField] internal ButtonComponentView marketplaceButton;
    [SerializeField] internal TMP_Text nftName;
    [SerializeField] internal TMP_Text nftNameMarketPlace;
    [SerializeField] internal GameObject marketplaceSection;
    [SerializeField] internal ImageComponentView nftImage;
    [SerializeField] internal ImageComponentView typeImage;
    [SerializeField] internal Image backgroundImage;
    [SerializeField] internal Image rarityBackgroundImage;
    [SerializeField] internal NFTTypeIconsAndColors nftTypesIcons;

    [SerializeField] internal NFTIconComponentModel model;

    public Button.ButtonClickedEvent onMarketplaceButtonClick => marketplaceButton?.onClick;

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
        SetMarketplaceURI(model.marketplaceURI);
        SetImageURI(model.imageURI);
        SetType(model.type);
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

        nftImage.SetImage(imageURI);
    }

    public void SetType(string type)
    {
        model.type = type;

        typeImage.SetImage(nftTypesIcons.GetTypeImage(type));
    }

    public void SetRarity(string rarity)
    {
        model.rarity = rarity;
        backgroundImage.color = nftTypesIcons.GetColor(rarity);
        rarityBackgroundImage.color = nftTypesIcons.GetColor(rarity);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        marketplaceSection.SetActive(true);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        marketplaceSection.SetActive(false);
    }

}
