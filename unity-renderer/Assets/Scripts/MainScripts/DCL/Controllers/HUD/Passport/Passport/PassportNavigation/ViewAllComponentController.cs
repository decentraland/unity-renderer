using Cysharp.Threading.Tasks;
using DCL.NotificationModel;
using DCL.Tasks;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Threading;
using Type = DCL.NotificationModel.Type;

public class ViewAllComponentController : IDisposable
{
    private const string NAME_TYPE = "name";
    private const string EMOTE_TYPE = "emote";
    private const string LAND_TYPE = "land";
    private const string REQUEST_ERROR_MESSAGE = "There was an error while trying to process your request. Please try again.";

    public event Action OnBackFromViewAll;
    public delegate void ClickBuyNftDelegate(string wearableId, string wearableType);
    public event ClickBuyNftDelegate OnClickBuyNft;


    private readonly IWearablesCatalogService wearablesCatalogService;
    private readonly IViewAllComponentView view;
    private readonly StringVariable currentPlayerId;
    private readonly ILandsService landsService;
    private readonly INamesService namesService;
    private readonly NotificationsController notificationsController;
    private CancellationTokenSource sectionsCts = new ();
    private bool cleanCachedWearablesPages;

    public ViewAllComponentController(
        IViewAllComponentView view,
        StringVariable currentPlayerId,
        IWearablesCatalogService wearablesCatalogService,
        ILandsService landsService,
        INamesService namesService,
        NotificationsController notificationsController)
    {
        this.view = view;
        this.currentPlayerId = currentPlayerId;
        this.wearablesCatalogService = wearablesCatalogService;
        this.landsService = landsService;
        this.namesService = namesService;
        this.notificationsController = notificationsController;
        view.OnBackFromViewAll += BackFromViewAll;
        view.OnRequestCollectibleElements += RequestCollectibleElements;
        view.OnClickBuyNft += (wearableId, wearableType) => OnClickBuyNft?.Invoke(wearableId, wearableType);
    }

    public void OpenViewAllSection(PassportSection section)
    {
        view.Initialize(section);
    }

    public void SetViewAllVisibility(bool isVisible)
    {
        view.SetVisible(isVisible);

        if (isVisible)
        {
            sectionsCts = sectionsCts.SafeRestart();
            cleanCachedWearablesPages = true;
        }
        else
        {
            sectionsCts.SafeCancelAndDispose();
            sectionsCts = null;

            view.SetLoadingActive(false);
        }
    }

    private void BackFromViewAll()
    {
        OnBackFromViewAll?.Invoke();
        SetViewAllVisibility(false);
        view.CloseAllNftItemInfos();
    }

    private void ShowNftIcons(List<(NFTIconComponentModel model, WearableItem w)> wearables)
    {
        view.ShowNftIcons(wearables);
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
        view.CloseAllNftItemInfos();

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
                await wearablesCatalogService.RequestOwnedWearablesAsync(userId, pageNumber, pageSize, cleanCachedWearablesPages, ct);

            cleanCachedWearablesPages = false;
            ProcessReceivedWearables(ownedWearableItems.wearables);
            view.SetTotalElements(ownedWearableItems.totalAmount);
            view.SetLoadingActive(false);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            ShowErrorAndGoBack();
        }
    }

    private async UniTask RequestOwnedNamesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
    {
        try
        {
            view.SetLoadingActive(true);

            using var pagePointer = namesService.GetPaginationPointer(userId, pageSize, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(pageNumber, ct);

            if (response.success)
            {
                ProcessReceivedNames(response.response.Names);
                view.SetTotalElements(response.response.TotalAmount);
                view.SetLoadingActive(false);
            }
            else
                ShowErrorAndGoBack();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            ShowErrorAndGoBack();
        }
    }

    private async UniTask RequestOwnedLandsAsync(string userId, int pageNumber, int pageSize, CancellationToken ct)
    {
        try
        {
            view.SetLoadingActive(true);

            using var pagePointer = landsService.GetPaginationPointer(userId, pageSize, CancellationToken.None);
            var response = await pagePointer.GetPageAsync(pageNumber, ct);

            if (response.success)
            {
                ProcessReceivedLands(response.response.Lands);
                view.SetTotalElements(response.response.TotalAmount);
                view.SetLoadingActive(false);
            }
            else
                ShowErrorAndGoBack();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            ShowErrorAndGoBack();
        }
    }

    private void ProcessReceivedWearables(IReadOnlyList<WearableItem> wearables)
    {
        List<(NFTIconComponentModel Model, WearableItem w)> wearableModels = new List<(NFTIconComponentModel Model, WearableItem w)>();
        foreach (var wearable in wearables)
        {
            wearableModels.Add((new NFTIconComponentModel
            {
                showMarketplaceButton = wearable.IsCollectible(),
                showType = wearable.IsCollectible(),
                type = wearable.data.category,
                marketplaceURI = "",
                name = wearable.GetName(),
                rarity = wearable.rarity,
                imageURI = wearable.ComposeThumbnailUrl(),
                nftId = (wearable.id, wearable.data.category)
            }, wearable));
        }
        ShowNftIcons(wearableModels);
    }

    private void ProcessReceivedEmotes(WearableItem[] emotes)
    {
        List<(NFTIconComponentModel Model, WearableItem w)> emoteModels = new List<(NFTIconComponentModel Model, WearableItem w)>();
        foreach (var emote in emotes)
        {
            emoteModels.Add((new NFTIconComponentModel
            {
                showMarketplaceButton = true,
                showType = true,
                type = EMOTE_TYPE,
                marketplaceURI = "",
                name = emote.GetName(),
                rarity = emote.rarity,
                imageURI = emote.ComposeThumbnailUrl(),
                nftId = (emote.id, EMOTE_TYPE)
            }, emote));
        }
        ShowNftIcons(emoteModels);
    }

    private void ProcessReceivedNames(IReadOnlyList<NamesResponse.NameEntry> names)
    {
        List<(NFTIconComponentModel Model, WearableItem w)> nameModels = new List<(NFTIconComponentModel Model, WearableItem w)>();
        foreach (var name in names)
        {
            nameModels.Add((new NFTIconComponentModel
            {
                showMarketplaceButton = true,
                showType = true,
                type = NAME_TYPE,
                marketplaceURI = "",
                name = name.Name,
                rarity = NAME_TYPE,
                imageURI = "",
                nftId = (name.ContractAddress, NAME_TYPE)
            }, null));
        }
        ShowNftIcons(nameModels);
    }

    private void ProcessReceivedLands(IReadOnlyList<LandsResponse.LandEntry> lands)
    {
        List<(NFTIconComponentModel Model, WearableItem w)> landModels = new List<(NFTIconComponentModel Model, WearableItem w)>();
        foreach (var land in lands)
        {
            landModels.Add((new()
            {
                showMarketplaceButton = true,
                showType = true,
                type = land.Category,
                marketplaceURI = "",
                name = !string.IsNullOrEmpty(land.Name) ? land.Name : land.Category,
                rarity = LAND_TYPE,
                imageURI = land.Image,
                nftId = (land.ContractAddress, land.Category)
            }, null));
        }
        ShowNftIcons(landModels);
    }

    private void ShowErrorAndGoBack()
    {
        notificationsController.ShowNotification(new Model
        {
            message = REQUEST_ERROR_MESSAGE,
            type = Type.ERROR,
            timer = 10f,
            destroyOnFinish = true,
        });

        BackFromViewAll();
    }
}
