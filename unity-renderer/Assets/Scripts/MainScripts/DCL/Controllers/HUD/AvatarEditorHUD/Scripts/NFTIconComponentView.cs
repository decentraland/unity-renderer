using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class NFTIconComponentView : BaseComponentView, INFTIconComponentView
{

    [SerializeField] internal ButtonComponentView marketplaceButton;
    [SerializeField] internal TMP_Text nftName;
    [SerializeField] internal TMP_Text nftNameMarketPlace;
    [SerializeField] internal GameObject marketplaceSection;
    [SerializeField] internal ImageComponentView nftImage;
    
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
