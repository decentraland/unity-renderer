using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCL;
using DCL.Helpers;
using DCL.Helpers.NFT;
using DCL.Interface;
using System.Collections;
using NFTShape_Internal;

internal interface INFTPromptHUDView : IDisposable
{
    event Action OnOwnerLabelPointerEnter;
    event Action OnOwnerLabelPointerExit;
    event Action OnOwnersTooltipFocusLost;
    event Action OnOwnersTooltipFocus;
    event Action OnViewAllPressed;
    event Action OnOwnersPopupClosed;
    void SetActive(bool active);
    bool IsActive();
    IOwnersTooltipView GetOwnersTooltip();
    IOwnersPopupView GetOwnersPopup();
    OwnerInfoElement GetOwnerElementPrefab();
    void SetLoading();
    void SetNFTInfo(NFTInfoSingleAsset info, string comment);
    void OnError(string error);
}

internal class NFTPromptHUDView : MonoBehaviour, INFTPromptHUDView
{
    private const string MULTIPLE_OWNERS_FORMAT = "{0} owners";
    private const int ADDRESS_MAX_CHARS = 11;

    public event Action OnOwnerLabelPointerEnter;
    public event Action OnOwnerLabelPointerExit;
    public event Action OnOwnersTooltipFocusLost;
    public event Action OnOwnersTooltipFocus;
    public event Action OnViewAllPressed;
    public event Action OnOwnersPopupClosed;

    [SerializeField] internal GameObject content;
    [SerializeField] internal GameObject nftContent;
    [SerializeField] internal GameObject mainErrorFeedbackContent;
    [SerializeField] internal GameObject imageErrorFeedbackContent;

    [SerializeField] RawImage imageNft;
    [SerializeField] Image imageNftBackground;
    [SerializeField] TextMeshProUGUI textNftName;
    [SerializeField] TextMeshProUGUI textOwner;
    [SerializeField] TextMeshProUGUI textMultipleOwner;
    [SerializeField] UIHoverCallback multipleOwnersContainer;

    [Header("Last Sale")] [SerializeField] TextMeshProUGUI textLastSaleSymbol;
    [SerializeField] TextMeshProUGUI textLastSalePrice;
    [SerializeField] TextMeshProUGUI textLastSaleNeverSold;

    [Header("Price")] [SerializeField] TextMeshProUGUI textPriceSymbol;
    [SerializeField] TextMeshProUGUI textPrice;
    [SerializeField] TextMeshProUGUI textPriceNotForSale;

    [Header("Description & Comment")]
    [SerializeField]
    TextMeshProUGUI textDescription;

    [SerializeField] TextMeshProUGUI textComment;
    [SerializeField] GameObject containerDescription;
    [SerializeField] GameObject containerComment;

    [Header("Spinners")] [SerializeField] GameObject spinnerGeneral;
    [SerializeField] GameObject spinnerNftImage;

    [Header("Buttons")] [SerializeField] internal Button buttonClose;
    [SerializeField] internal Button buttonCancel;
    [SerializeField] internal Button buttonOpenMarket;
    [SerializeField] TextMeshProUGUI textOpenMarketButton;

    [Header("Owners")]
    [SerializeField] internal OwnerInfoElement ownerElementPrefab;
    [SerializeField] internal OwnersTooltipView ownersTooltip;
    [SerializeField] internal OwnersPopupView ownersPopup;

    Coroutine fetchNFTImageRoutine = null;

    private string nftTokenId;
    bool backgroundColorSet = false;
    string marketUrl = null;

    private bool isDestroyed = false;
    internal INFTAssetRetriever nftAssetRetriever;
    private INFTAsset nftAsset;

    private void Awake()
    {
        name = "_NFTPromptHUD";

        buttonClose.onClick.AddListener(Hide);
        buttonCancel.onClick.AddListener(Hide);
        buttonOpenMarket.onClick.AddListener(OpenMarketUrl);

        multipleOwnersContainer.OnPointerEnter += OwnerLabelPointerEnter;
        multipleOwnersContainer.OnPointerExit += OwnerLabelPointerExit;
        ownersTooltip.OnViewAllPressed += OnViewAllOwnersPressed;
        ownersTooltip.OnFocusLost += OnOwnersTooltipLostFocus;
        ownersTooltip.OnFocus += OnOwnersTooltipGainFocus;
        ownersPopup.OnClosePopup += OnOwnersPopupClose;
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }

        nftAssetRetriever?.Dispose();
        nftAsset?.Dispose();
    }

    internal void Hide()
    {
        content.SetActive(false);

        nftAssetRetriever?.Dispose();
        nftAsset?.Dispose();

        if (fetchNFTImageRoutine != null)
            StopCoroutine(fetchNFTImageRoutine);

        fetchNFTImageRoutine = null;

        AudioScriptableObjects.dialogClose.Play(true);
    }

    IOwnersPopupView INFTPromptHUDView.GetOwnersPopup() { return ownersPopup; }

    IOwnersTooltipView INFTPromptHUDView.GetOwnersTooltip() { return ownersTooltip; }

    void INFTPromptHUDView.SetActive(bool active) { content.SetActive(active); }

    bool INFTPromptHUDView.IsActive() { return content.activeSelf; }

    OwnerInfoElement INFTPromptHUDView.GetOwnerElementPrefab() { return ownerElementPrefab; }

    void INFTPromptHUDView.SetLoading()
    {
        Show();

        if (fetchNFTImageRoutine != null)
            StopCoroutine(fetchNFTImageRoutine);

        SetTransparentBackground();

        imageNft.gameObject.SetActive(false);
        textNftName.gameObject.SetActive(false);
        textOwner.gameObject.SetActive(false);
        multipleOwnersContainer.gameObject.SetActive(false);
        textLastSaleSymbol.gameObject.SetActive(false);
        textLastSalePrice.gameObject.SetActive(false);
        textLastSaleNeverSold.gameObject.SetActive(false);
        textPriceSymbol.gameObject.SetActive(false);
        textPrice.gameObject.SetActive(false);
        textPriceNotForSale.gameObject.SetActive(false);
        containerDescription.SetActive(false);
        containerComment.SetActive(false);
        buttonCancel.gameObject.SetActive(false);
        buttonOpenMarket.gameObject.SetActive(false);

        nftContent.SetActive(false);
        ShowImageLoading(false);
        ShowMainLoading(true);
        ShowMainErrorFeedback(false);
    }

    void INFTPromptHUDView.SetNFTInfo(NFTInfoSingleAsset info, string comment)
    {
        Show();

        ShowMainLoading(false);
        nftContent.SetActive(true);

        nftTokenId = info.tokenId;
        SetTransparentBackground();
        backgroundColorSet = info.backgroundColor != null;
        if (backgroundColorSet)
        {
            imageNftBackground.color = info.backgroundColor.Value;
        }

        textNftName.text = info.name;
        textNftName.gameObject.SetActive(true);

        bool hasMultipleOwners = info.owners.Length > 1;
        if (hasMultipleOwners)
        {
            textMultipleOwner.text = string.Format(MULTIPLE_OWNERS_FORMAT, info.owners.Length);
        }
        else
        {
            textOwner.text = info.owners.Length == 1
                ? NFTPromptHUDController.FormatOwnerAddress(info.owners[0].owner, ADDRESS_MAX_CHARS)
                : NFTPromptHUDController.FormatOwnerAddress("0x0000000000000000000000000000000000000000", ADDRESS_MAX_CHARS);
        }
        textOwner.gameObject.SetActive(!hasMultipleOwners);
        multipleOwnersContainer.gameObject.SetActive(hasMultipleOwners);

        if (!string.IsNullOrEmpty(info.lastSaleAmount))
        {
            textLastSalePrice.text = ShortDecimals(info.lastSaleAmount, 4);
            textLastSalePrice.gameObject.SetActive(true);
        }
        else
        {
            textLastSaleNeverSold.gameObject.SetActive(true);
        }

        if (!string.IsNullOrEmpty(info.currentPrice))
        {
            textPrice.text = ShortDecimals(info.currentPrice, 4);
            textPrice.gameObject.SetActive(true);

            if (info.currentPriceToken != null)
            {
                SetTokenSymbol(textPriceSymbol, info.currentPriceToken.Value.symbol);
            }
        }
        else
        {
            textPriceNotForSale.gameObject.SetActive(true);
        }

        if (info.lastSaleToken != null)
        {
            SetTokenSymbol(textLastSaleSymbol, info.lastSaleToken.Value.symbol);
        }

        if (!string.IsNullOrEmpty(info.description))
        {
            textDescription.text = info.description;
            containerDescription.SetActive(true);
        }

        if (!string.IsNullOrEmpty(comment))
        {
            textComment.text = comment;
            containerComment.SetActive(true);
        }

        textOpenMarketButton.text = "VIEW";
        if (info.marketInfo != null)
        {
            textOpenMarketButton.text = $"{textOpenMarketButton.text} ON {info.marketInfo.Value.name.ToUpper()}";
        }

        marketUrl = null;
        if (!string.IsNullOrEmpty(info.marketLink))
        {
            marketUrl = info.marketLink;
        }
        else if (!string.IsNullOrEmpty(info.assetLink))
        {
            marketUrl = info.assetLink;
        }

        buttonCancel.gameObject.SetActive(true);
        buttonOpenMarket.gameObject.SetActive(true);

        fetchNFTImageRoutine = StartCoroutine(FetchNFTImage(info));
    }

    private void Show()
    {
        content.SetActive(true);
        Utils.UnlockCursor();
    }

    private IEnumerator FetchNFTImage(NFTInfoSingleAsset nftInfo)
    {
        ShowImageErrorFeedback(false);
        ShowImageLoading(true);

        nftAssetRetriever?.Dispose();
        nftAsset?.Dispose();
        
        nftAssetRetriever = new NFTAssetRetriever();
        yield return nftAssetRetriever.LoadNFTAsset(
            nftInfo.previewImageUrl,
            OnSuccess: nftAsset =>
            {
                this.nftAsset = nftAsset;
                nftAsset.OnTextureUpdate += UpdateTexture;

                if (!(nftAsset is Asset_Gif))
                {
                    if (!backgroundColorSet)
                    {
                        SetTransparentBackground();
                    }
                }

                UpdateTexture(nftAsset.previewAsset.texture);
                SetNFTImageSize(nftAsset.previewAsset.texture);
                imageNft.gameObject.SetActive(true);
                ShowImageLoading(false);
            },
            OnFail:
            (exc) => { ShowImageErrorFeedback(true); });
    }

    private void UpdateTexture(Texture2D texture)
    {
        imageNft.texture = texture;
    }

    private void SetNFTImageSize(Texture2D texture)
    {
        RectTransform rt = (RectTransform)imageNft.transform.parent;
        
        float h, w;
        
        if (texture.height > texture.width)
        {
            h = rt.rect.height;
            w = h * (texture.width / (float)texture.height);
        }
        else
        {
            w = rt.rect.width;
            h = w * (texture.height / (float)texture.width);
        }

        imageNft.rectTransform.sizeDelta = new Vector2(w, h);
    }

    private string ShortDecimals(string value, int decimalCount)
    {
        int pointPosition = value.IndexOf('.');
        
        if (pointPosition <= 0)
            return value;

        string ret = value.Substring(0, pointPosition + Mathf.Min(value.Length - pointPosition, decimalCount + 1));

        for (int i = ret.Length - 1; i >= 0; i--)
        {
            if (ret[i] == '.')
            {
                return ret.Substring(0, i);
            }

            if (ret[i] != '0')
            {
                return ret.Substring(0, i + 1);
            }
        }

        return ret;
    }

    private void SetTransparentBackground()
    {
        imageNftBackground.color = new Color(
            imageNftBackground.color.r,
            imageNftBackground.color.g,
            imageNftBackground.color.b,
            0f);
    }

    private void SetTokenSymbol(TextMeshProUGUI textToken, string symbol)
    {
        textToken.text = symbol;
        textToken.gameObject.SetActive(true);
    }

    private void OpenMarketUrl()
    {
        if (!string.IsNullOrEmpty(marketUrl))
        {
            WebInterface.OpenURL(marketUrl);
            AnalyticsHelper.SendExternalLinkAnalytic(marketUrl, nftTokenId);
        }
        else
        {
            Hide();
        }
    }

    void INFTPromptHUDView.OnError(string error)
    {
        Debug.LogError(error);
        ShowMainErrorFeedback(true);
    }

    private void OnDestroy()
    {
        isDestroyed = true;

        multipleOwnersContainer.OnPointerEnter -= OwnerLabelPointerEnter;
        multipleOwnersContainer.OnPointerExit -= OwnerLabelPointerExit;
        ownersTooltip.OnViewAllPressed -= OnViewAllOwnersPressed;
        ownersTooltip.OnFocusLost -= OnOwnersTooltipLostFocus;
        ownersTooltip.OnFocus -= OnOwnersTooltipGainFocus;
        ownersPopup.OnClosePopup -= OnOwnersPopupClose;

        nftAssetRetriever?.Dispose();
        nftAsset?.Dispose();
    }

    private void OnViewAllOwnersPressed() { OnViewAllPressed?.Invoke(); }

    private void OnOwnersTooltipGainFocus() { OnOwnersTooltipFocus?.Invoke(); }

    private void OnOwnersTooltipLostFocus() { OnOwnersTooltipFocusLost?.Invoke(); }

    private void OnOwnersPopupClose() { OnOwnersPopupClosed?.Invoke(); }

    private void OwnerLabelPointerEnter() { OnOwnerLabelPointerEnter?.Invoke(); }

    private void OwnerLabelPointerExit() { OnOwnerLabelPointerExit?.Invoke(); }

    private void ShowMainLoading(bool isVisible)
    {
        if (spinnerGeneral == null)
            return;

        spinnerGeneral.SetActive(isVisible);
    }

    private void ShowMainErrorFeedback(bool isVisible)
    {
        if (mainErrorFeedbackContent == null)
            return;

        if (isVisible)
            ShowMainLoading(false);

        mainErrorFeedbackContent.SetActive(isVisible);
    }

    private void ShowImageLoading(bool isVisible)
    {
        if (spinnerNftImage == null)
            return;

        spinnerNftImage.SetActive(isVisible);
    }

    private void ShowImageErrorFeedback(bool isVisible)
    {
        if (imageErrorFeedbackContent == null)
            return;

        if (isVisible)
            ShowImageLoading(false);

        imageErrorFeedbackContent.SetActive(isVisible);
    }
}