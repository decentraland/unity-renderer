using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewAllComponentController : IDisposable
{
    private const string NAME_TYPE = "name";
    private const string EMOTE_TYPE = "emote";
    private const string LAND_TYPE = "land";

    public event Action OnBackFromViewAll;

    private readonly IViewAllComponentView view;

    public ViewAllComponentController(IViewAllComponentView view)
    {
        this.view = view;
        view.OnBackFromViewAll += BackFromViewAll;
        view.OnRequestCollectibleElements += RequestCollectibleElements;
    }

    private void RequestCollectibleElements(string type, int pageNumber, int pageSize)
    {
        //TODO: Wire request to new refactored implementation
    }

    private void ProcessReceivedWearables(WearableItem[] wearables)
    {
        List<NFTIconComponentModel> wearableModels = new List<NFTIconComponentModel>();
        foreach (var wearable in wearables)
        {
            wearableModels.Add(new NFTIconComponentModel
            {
                showMarketplaceButton = wearable.IsCollectible(),
                showType = wearable.IsCollectible(),
                type = wearable.data.category,
                marketplaceURI = "",
                name = wearable.GetName(),
                rarity = wearable.rarity,
                imageURI = wearable.ComposeThumbnailUrl()
            });
        }
        ShowNftIcons(wearableModels);
    }

    private void ProcessReceivedEmotes(WearableItem[] emotes)
    {
        List<NFTIconComponentModel> emoteModels = new List<NFTIconComponentModel>();
        foreach (var emote in emotes)
        {
            emoteModels.Add(
                new NFTIconComponentModel
                {
                    showMarketplaceButton = true,
                    showType = true,
                    type = EMOTE_TYPE,
                    marketplaceURI = "",
                    name = emote.GetName(),
                    rarity = emote.rarity,
                    imageURI = emote.ComposeThumbnailUrl(),
                    nftId = (emote.id, EMOTE_TYPE)
                });
        }
        ShowNftIcons(emoteModels);
    }

    private void ProcessReceivedNames(NamesResponse.NameEntry[] names)
    {
        List<NFTIconComponentModel> nameModels = new List<NFTIconComponentModel>();
        foreach (var name in names)
        {
            nameModels.Add(new NFTIconComponentModel
            {
                showMarketplaceButton = true,
                showType = true,
                type = NAME_TYPE,
                marketplaceURI = "",
                name = name.Name,
                rarity = NAME_TYPE,
                imageURI = "",
                nftId = (name.ContractAddress, NAME_TYPE)
            });
        }
        ShowNftIcons(nameModels);
    }

    private void ProcessReceivedLands(LandsResponse.LandEntry[] lands)
    {
        List<NFTIconComponentModel> landModels = new List<NFTIconComponentModel>();
        for (int i = 0; i < lands.Length; i++)
        {
            landModels.Add(new()
            {
                showMarketplaceButton = true,
                showType = true,
                type = lands[i].Category,
                marketplaceURI = "",
                name = !string.IsNullOrEmpty(lands[i].Name) ? lands[i].Name : lands[i].Category,
                rarity = LAND_TYPE,
                imageURI = lands[i].Image,
                nftId = (lands[i].ContractAddress, lands[i].Category)
            });
        }
        ShowNftIcons(landModels);
    }


    public void OpenViewAllSection(string sectionName)
    {
        view.Initialize(300);
        view.SetSectionName(sectionName);
    }

    public void SetViewAllVisibility(bool isVisible)
    {
        view.SetVisible(isVisible);
    }

    private void BackFromViewAll()
    {
        OnBackFromViewAll?.Invoke();
        SetViewAllVisibility(false);
    }

    private void ShowNftIcons(List<NFTIconComponentModel> models)
    {
        view.ShowNftIcons(models);
    }

    public void Dispose()
    {
        view.OnBackFromViewAll -= BackFromViewAll;
    }
}
