using Cysharp.Threading.Tasks;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ViewAllComponentController : IDisposable
{
    private const string NAME_TYPE = "name";
    private const string EMOTE_TYPE = "emote";
    private const string LAND_TYPE = "land";

    public event Action OnBackFromViewAll;

    private readonly IWearablesCatalogService wearablesCatalogService;
    private readonly IViewAllComponentView view;
    private readonly StringVariable currentPlayerId;
    private readonly ILandsService landsService;
    private readonly INamesService namesService;

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
    }

    private void RequestCollectibleElements(string type, int pageNumber, int pageSize)
    {
        async UniTask RequestOwnedWearablesAsync(CancellationToken ct)
        {
            IReadOnlyList<WearableItem> ownedWearableItems = await wearablesCatalogService.RequestOwnedWearablesAsync(currentPlayerId, pageNumber, pageSize, true, CancellationToken.None);
            ProcessReceivedWearables(ownedWearableItems.ToArray());
        }

        async UniTask RequestOwnedNamesAsync(CancellationToken ct)
        {
            using var pagePointer = namesService.GetPaginationPointer(currentPlayerId, pageSize, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(pageNumber, ct);
            var namesResult = Array.Empty<NamesResponse.NameEntry>();

            if (response.success)
                namesResult = response.response.Names.ToArray();

            ProcessReceivedNames(namesResult);
        }

        async UniTask RequestOwnedLandsAsync(CancellationToken ct)
        {
            using var pagePointer = landsService.GetPaginationPointer(currentPlayerId, pageSize, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(pageNumber, ct);
            var landsResult = Array.Empty<LandsResponse.LandEntry>();

            if (response.success)
                landsResult = response.response.Lands.ToArray();

            ProcessReceivedLands(landsResult);
        }

        switch (type)
        {
            case "wearables":
                RequestOwnedWearablesAsync(CancellationToken.None).Forget();
                break;
            case "names":
                RequestOwnedNamesAsync(CancellationToken.None).Forget();
                break;
            case "lands":
                RequestOwnedLandsAsync(CancellationToken.None).Forget();
                break;
        }
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
        view.Initialize(sectionName, 300);
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
