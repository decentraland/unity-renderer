using DCL;
using DCL.Helpers.NFT;
using UnityEngine;

public class NFTPromptHUDController : IHUD
{
    internal const string VIEW_PREFAB_PATH = "NFTPromptHUD";

    internal INFTPromptHUDView view { get; private set; }

    private readonly OwnersInfoController ownersInfoController;

    private Coroutine fetchNFTRoutine = null;
    private NFTInfoSingleAsset? lastNFTInfo = null;

    private bool isPointerInTooltipArea = false;
    private bool isPointerInOwnerArea = false;

    public NFTPromptHUDController()
    {
        view = Object.Instantiate(Resources.Load<GameObject>(VIEW_PREFAB_PATH))
                     .GetComponent<NFTPromptHUDView>();
        view.SetActive(false);

        view.OnOwnerLabelPointerEnter += ShowOwnersTooltip;
        view.OnOwnerLabelPointerExit += TryHideOwnersTooltip;
        view.OnOwnersTooltipFocusLost += OnOwnersTooltipFocusLost;
        view.OnOwnersTooltipFocus += OnOwnersTooltipFocus;
        view.OnViewAllPressed += ShowOwnersPopup;
        view.OnOwnersPopupClosed += HideOwnersPopup;

        ownersInfoController = new OwnersInfoController(view.GetOwnerElementPrefab());
        DataStore.i.onOpenNFTPrompt.OnChange += OpenNftInfoDialog;
    }

    public void OpenNftInfoDialog(NFTPromptModel model, NFTPromptModel prevModel)
    {
        if (!view.IsActive())
        {
            HideOwnersTooltip(true);
            HideOwnersPopup(true);

            if (fetchNFTRoutine != null)
                CoroutineStarter.Stop(fetchNFTRoutine);

            if (lastNFTInfo != null && lastNFTInfo.Value.Equals(model.contactAddress, model.tokenId))
            {
                SetNFT(lastNFTInfo.Value, model.comment, false);
                return;
            }

            view.SetLoading();

            fetchNFTRoutine = CoroutineStarter.Start(NFTHelper.FetchNFTInfoSingleAsset(model.contactAddress, model.tokenId,
                (nftInfo) => SetNFT(nftInfo, model.comment, true),
                (error) => view.OnError(error)
            ));
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetActive(visible);

        if (!visible)
        {
            HideOwnersTooltip(true);
            HideOwnersPopup(true);
        }

        AudioScriptableObjects.dialogOpen.Play(true);
    }

    public void Dispose()
    {
        view.OnOwnerLabelPointerEnter -= ShowOwnersTooltip;
        view.OnOwnerLabelPointerExit -= TryHideOwnersTooltip;
        view.OnOwnersTooltipFocusLost -= OnOwnersTooltipFocusLost;
        view.OnOwnersTooltipFocus -= OnOwnersTooltipFocus;
        view.OnViewAllPressed -= ShowOwnersPopup;
        view.OnOwnersPopupClosed -= HideOwnersPopup;

        CoroutineStarter.Stop(fetchNFTRoutine);

        view?.Dispose();

        DataStore.i.onOpenNFTPrompt.OnChange -= OpenNftInfoDialog;
    }

    private void SetNFT(NFTInfoSingleAsset info, string comment, bool shouldRefreshOwners)
    {
        lastNFTInfo = info;
        view.SetNFTInfo(info, comment);
        if (shouldRefreshOwners)
        {
            ownersInfoController.SetOwners(info.owners);
        }
    }

    private void ShowOwnersTooltip()
    {
        isPointerInOwnerArea = true;

        if (lastNFTInfo == null || lastNFTInfo.Value.owners.Length <= 1)
            return;

        var tooltip = view.GetOwnersTooltip();

        if (tooltip.IsActive())
        {
            tooltip.KeepOnScreen();
            return;
        }

        tooltip.SetElements(ownersInfoController.GetElements());
        tooltip.Show();
    }

    void TryHideOwnersTooltip()
    {
        isPointerInOwnerArea = false;

        if (!isPointerInTooltipArea)
        {
            HideOwnersTooltip();
        }
    }

    void OnOwnersTooltipFocusLost()
    {
        isPointerInTooltipArea = false;
        if (!isPointerInOwnerArea)
        {
            HideOwnersTooltip();
        }
    }

    void OnOwnersTooltipFocus()
    {
        isPointerInTooltipArea = true;
        var tooltip = view.GetOwnersTooltip();
        if (tooltip.IsActive())
        {
            tooltip.KeepOnScreen();
        }
    }

    private void HideOwnersTooltip() { HideOwnersTooltip(false); }

    private void HideOwnersTooltip(bool instant)
    {
        var tooltip = view.GetOwnersTooltip();
        if (!tooltip.IsActive())
            return;

        tooltip.Hide(instant);
    }

    private void ShowOwnersPopup()
    {
        HideOwnersTooltip(true);
        isPointerInTooltipArea = false;
        isPointerInOwnerArea = false;

        var popup = view.GetOwnersPopup();

        if (popup.IsActive())
            return;

        popup.SetElements(ownersInfoController.GetElements());
        popup.Show();
    }

    private void HideOwnersPopup() { HideOwnersPopup(false); }

    private void HideOwnersPopup(bool instant) { view.GetOwnersPopup().Hide(instant); }

    internal static string FormatOwnerAddress(string address, int maxCharacters)
    {
        const string ellipsis = "...";

        if (address.Length <= maxCharacters)
        {
            return address;
        }
        else
        {
            int segmentLength = Mathf.FloorToInt((maxCharacters - ellipsis.Length) * 0.5f);
            return $"{address.Substring(0, segmentLength)}{ellipsis}{address.Substring(address.Length - segmentLength, segmentLength)}";
        }
    }
}