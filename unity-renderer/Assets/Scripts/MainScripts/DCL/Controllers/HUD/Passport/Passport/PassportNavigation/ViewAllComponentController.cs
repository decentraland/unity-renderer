using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ViewAllComponentController : IDisposable
{
    private const string NAME_TYPE = "name";
    private const string EMOTE_TYPE = "emote";
    private const string LAND_TYPE = "land";

    public event Action OnBackFromViewAll;
    public event Action<string, string> OnClickBuyNft;

    private readonly IWearablesCatalogService wearablesCatalogService;
    private readonly IViewAllComponentView view;
    private readonly StringVariable currentPlayerId;
    private readonly ILandsService landsService;
    private readonly INamesService namesService;
    private CancellationTokenSource sectionsCts = new ();

    public ViewAllComponentController(
        IViewAllComponentView view,
        StringVariable currentPlayerId,
        IWearablesCatalogService wearablesCatalogService,
        ILandsService landsService,
        INamesService namesService)
    {
        this.view = view;
        this.currentPlayerId = currentPlayerId;
        this.wearablesCatalogService = wearablesCatalogService;
        this.landsService = landsService;
        this.namesService = namesService;
        view.OnBackFromViewAll += BackFromViewAll;
        view.OnRequestCollectibleElements += RequestCollectibleElements;
        view.OnClickBuyNft += (s1, s2) => OnClickBuyNft?.Invoke(s1, s2);
    }

    public void OpenViewAllSection(PassportSection section)
    {
        view.Initialize(section);
    }

    public void SetViewAllVisibility(bool isVisible)
    {
        view.SetVisible(isVisible);

        if (!isVisible)
        {
            sectionsCts.SafeCancelAndDispose();
            sectionsCts = null;
        }
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
        view.OnRequestCollectibleElements -= RequestCollectibleElements;

        sectionsCts.SafeCancelAndDispose();
        sectionsCts = null;
    }

    private void RequestCollectibleElements(PassportSection section, int pageNumber, int pageSize)
    {
        sectionsCts = sectionsCts.SafeRestart();

        switch (section)
        {
            case PassportSection.Wearables:
                RequestOwnedWearablesAsync(currentPlayerId, pageNumber, pageSize, sectionsCts.Token).Forget();
                break;
            case PassportSection.Names:
                RequestOwnedNamesAsync(currentPlayerId, pageNumber, pageSize, sectionsCts.Token).Forget();
                break;
            case PassportSection.Lands:
                RequestOwnedLandsAsync(currentPlayerId, pageNumber, pageSize, sectionsCts.Token).Forget();
                break;
        }
    }

    private async UniTask RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
    {
        try
        {
            view.SetLoadingActive(true);

            (IReadOnlyList<WearableItem> wearables, int totalAmount) ownedWearableItems =
                await wearablesCatalogService.RequestOwnedWearablesAsync(userId, pageNumber, pageSize, true, ct);

            ProcessReceivedWearables(ownedWearableItems.wearables.ToArray());
            view.SetTotalElements(ownedWearableItems.totalAmount);
            view.SetLoadingActive(false);
        }
        catch (Exception)
        {
            view.ShowError();
        }
    }

    private async UniTask RequestOwnedNamesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
    {
        view.SetLoadingActive(true);

        using var pagePointer = namesService.GetPaginationPointer(userId, pageSize, CancellationToken.None);
        var response = await pagePointer.GetPageAsync(pageNumber, ct);

        if (response.success)
        {
            ProcessReceivedNames(response.response.Names.ToArray());
            view.SetTotalElements(response.response.TotalAmount);
            view.SetLoadingActive(false);
        }
        else
        {
            view.ShowError();
        }
    }

    private async UniTask RequestOwnedLandsAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
    {
        view.SetLoadingActive(true);

        using var pagePointer = landsService.GetPaginationPointer(userId, pageSize, CancellationToken.None);
        var response = await pagePointer.GetPageAsync(pageNumber, ct);

        if (response.success)
        {
            ProcessReceivedLands(response.response.Lands.ToArray());
            view.SetTotalElements(response.response.TotalAmount);
            view.SetLoadingActive(false);
        }
        else
        {
            view.ShowError();
        }

        view.SetLoadingActive(false);
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
                imageURI = wearable.ComposeThumbnailUrl(),
                nftId = (wearable.id, wearable.data.category)
            });
        }
        ShowNftIcons(wearableModels);
    }

    private void ProcessReceivedEmotes(WearableItem[] emotes)
    {
        List<NFTIconComponentModel> emoteModels = new List<NFTIconComponentModel>();
        foreach (var emote in emotes)
        {
            emoteModels.Add(new NFTIconComponentModel
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
}
