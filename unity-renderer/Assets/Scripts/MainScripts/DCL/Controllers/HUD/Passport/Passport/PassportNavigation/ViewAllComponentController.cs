using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewAllComponentController : IDisposable
{
    private const string NAME_TYPE = "name";

    public event Action OnBackFromViewAll;
    public event Action OnResultWearables;
    public event Action OnResultEmotes;
    public event Action OnResultNames;
    public event Action OnResultLands;

    private IViewAllComponentView view;

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
        for (int i = 0; i < wearables.Length; i++)
        {

        }
        ShowNftIcons(wearableModels);
    }

    private void ProcessReceivedEmotes(WearableItem[] emotes)
    {
        List<NFTIconComponentModel> emoteModels = new List<NFTIconComponentModel>();
        for (int i = 0; i < emotes.Length; i++)
        {

        }
        ShowNftIcons(emoteModels);
    }

    private void ProcessReceivedNames(NamesResponse.NameEntry[] names)
    {
        List<NFTIconComponentModel> nameModels = new List<NFTIconComponentModel>();
        for (int i = 0; i < names.Length; i++)
        {
            nameModels.Add(new NFTIconComponentModel
            {
                showMarketplaceButton = true,
                showType = true,
                type = NAME_TYPE,
                marketplaceURI = "",
                name = names[i].Name,
                rarity = NAME_TYPE,
                imageURI = "",
                nftId = (names[i].ContractAddress, NAME_TYPE)
            });
        }
        ShowNftIcons(nameModels);
    }

    private void ProcessReceivedLands(LandsResponse.LandEntry[] lands)
    {
        List<NFTIconComponentModel> landModels = new List<NFTIconComponentModel>();
        for (int i = 0; i < lands.Length; i++)
        {

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

    public void ShowNftIcons(List<NFTIconComponentModel> models)
    {
        view.ShowNftIcons(models);
    }

    public void Dispose()
    {
        view.OnBackFromViewAll -= BackFromViewAll;
    }
}
