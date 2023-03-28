using System;
using DCL;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ViewAllComponentView : BaseComponentView, IViewAllComponentView
{
    private const int ELEMENTS_PER_PAGE = 20;
    private const string NFT_ELEMENTS_POOL_NAME_PREFIX = "NFTElementsEntriesPool_";
    private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.75f, 0.75f, 0.75f);

    [SerializeField] private TMP_Text sectionName;
    [SerializeField] private TMP_Text sectionAmount;
    [SerializeField] private ButtonComponentView backButton;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private UIPageSelector pageSelector;
    [SerializeField] private GameObject nftPageElement;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] internal NFTItemInfo nftItemInfo;

    public event Action OnBackFromViewAll;
    public event Action<PassportSection, int, int> OnRequestCollectibleElements;
    public event Action<NftInfo> OnClickBuyNft;

    private List<PoolableObject> nftElementsPoolableQueue = new List<PoolableObject>();
    private Pool nftElementsEntryPool;
    private PassportSection section;
    private readonly List<NFTIconComponentView> nftWearableViews = new List<NFTIconComponentView>();
    private string poolEntryId;

    public override void Awake()
    {
        base.Awake();

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>
        {
            ClearNftPool();
            OnBackFromViewAll?.Invoke();
        });
        pageSelector.OnValueChanged += RequestPage;

        poolEntryId = NFT_ELEMENTS_POOL_NAME_PREFIX + name + GetInstanceID();
    }

    public void Initialize(PassportSection passportSection)
    {
        nftElementsEntryPool = GetNftElementsEntryPool();
        section = passportSection;
        sectionName.text = passportSection.ToString();
        sectionAmount.gameObject.SetActive(false);
        pageSelector.SelectPage(0);
    }

    public void SetTotalElements(int totalElements)
    {
        pageSelector.Setup((totalElements + ELEMENTS_PER_PAGE - 1) / ELEMENTS_PER_PAGE);
        sectionAmount.text = $"({totalElements})";
        if (!sectionAmount.gameObject.activeSelf)
            sectionAmount.gameObject.SetActive(true);
    }

    private void RequestPage(int pageNumber)
    {
        ClearNftPool();
        OnRequestCollectibleElements?.Invoke(section, pageNumber + 1, ELEMENTS_PER_PAGE);
    }

    public override void RefreshControl() { }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void ShowNftIcons(List<(NFTIconComponentModel model, WearableItem wearable)> iconsWithWearables)
    {
        nftWearableViews.Clear();

        foreach (var wearableData in iconsWithWearables)
        {
            PoolableObject poolableObject = nftElementsEntryPool.Get();
            nftElementsPoolableQueue.Add(poolableObject);
            poolableObject.gameObject.transform.SetParent(itemsContainer, false);
            poolableObject.gameObject.transform.localScale = NFT_ICON_SCALE;
            NFTIconComponentView nftIconComponentView = poolableObject.gameObject.GetComponent<NFTIconComponentView>();
            nftIconComponentView.onMarketplaceButtonClick.RemoveAllListeners();
            nftIconComponentView.onMarketplaceButtonClick.AddListener(() => ClickOnBuyWearable(wearableData.model.nftInfo));
            nftIconComponentView.Configure(wearableData.model);
            nftIconComponentView.onFocused -= FocusOnNFTIconView;
            nftIconComponentView.onFocused += FocusOnNFTIconView;

            if (wearableData.wearable != null)
            {
                nftIconComponentView.onDetailInfoButtonClick.AddListener(() => nftIconComponentView.SetNFTItemInfoActive(true));
                nftIconComponentView.ConfigureNFTItemInfo(nftItemInfo, wearableData.wearable, !wearableData.wearable.IsEmote());
            }
            else
                nftIconComponentView.onDetailInfoButtonClick.RemoveAllListeners();

            nftWearableViews.Add(nftIconComponentView);
        }
    }

    public void SetLoadingActive(bool isLoading)
    {
        loadingSpinner.SetActive(isLoading);
        itemsContainer.gameObject.SetActive(!isLoading);
    }

    public void CloseAllNftItemInfos()
    {
        foreach (var nftView in nftWearableViews)
            nftView.SetNFTItemInfoActive(false);
    }

    private void FocusOnNFTIconView(bool isFocused)
    {
        if (!isFocused)
            return;

        CloseAllNftItemInfos();
    }

    private void ClickOnBuyWearable(NftInfo nftInfo)
    {
        OnClickBuyNft?.Invoke(nftInfo);
    }

    private Pool GetNftElementsEntryPool()
    {
        var pool = PoolManager.i.GetPool(poolEntryId);
        if (pool != null) return pool;

        pool = PoolManager.i.AddPool(
            poolEntryId,
            Instantiate(nftPageElement).gameObject,
            maxPrewarmCount: ELEMENTS_PER_PAGE,
            isPersistent: true);

        pool.ForcePrewarm();
        return pool;
    }

    private void ClearNftPool()
    {
        foreach (var poolObject in nftElementsPoolableQueue)
            nftElementsEntryPool.Release(poolObject);

        nftElementsPoolableQueue = new List<PoolableObject>();
    }
}
