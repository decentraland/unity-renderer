using System;
using DCL;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ViewAllComponentView : BaseComponentView, IViewAllComponentView
{
    private const int ELEMENTS_PER_PAGE = 20;
    private const string NFT_ELEMENTS_POOL_NAME_PREFIX = "NFTElementsEntriesPool_";
    private const string ERROR_MESSAGE = "An error was encountered when loading the {section}. \n Please try again shortly";
    private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.75f, 0.75f, 0.75f);

    [SerializeField] private TMP_Text sectionName;
    [SerializeField] private TMP_Text sectionAmount;
    [SerializeField] private ButtonComponentView backButton;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private UIPageSelector pageSelector;
    [SerializeField] private GameObject nftPageElement;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TMP_Text errorText;

    public event Action OnBackFromViewAll;
    public event Action<PassportSection, int, int> OnRequestCollectibleElements;
    public event Action<string, string> OnClickBuyNft;

    private List<PoolableObject> nftElementsPoolableQueue = new List<PoolableObject>();
    private Pool nftElementsEntryPool;
    private PassportSection section;

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
    }

    public void Initialize(PassportSection passportSection)
    {
        errorPanel.SetActive(false);
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

    public void ShowError()
    {
        errorPanel.SetActive(true);
        errorText.text = ERROR_MESSAGE.Replace("{section}", section.ToString());
    }

    public void ShowNftIcons(List<NFTIconComponentModel> models)
    {
        foreach (var model in models)
        {
            PoolableObject poolableObject = nftElementsEntryPool.Get();
            nftElementsPoolableQueue.Add(poolableObject);
            poolableObject.gameObject.transform.SetParent(itemsContainer, false);
            poolableObject.gameObject.transform.localScale = NFT_ICON_SCALE;
            NFTIconComponentView nftIconComponentView = poolableObject.gameObject.GetComponent<NFTIconComponentView>();
            nftIconComponentView.onMarketplaceButtonClick.RemoveAllListeners();
            nftIconComponentView.onMarketplaceButtonClick.AddListener(() => ClickOnBuyWearable(model.nftId.Item1, model.nftId.Item2));
            nftIconComponentView.Configure(model);
        }
    }

    private void ClickOnBuyWearable(string wearableId, string wearableType)
    {
        OnClickBuyNft?.Invoke(wearableId, wearableType);
    }

    public void SetLoadingActive(bool isLoading)
    {
        loadingSpinner.SetActive(isLoading);
        itemsContainer.gameObject.SetActive(!isLoading);
    }

    private Pool GetNftElementsEntryPool()
    {
        var pool = PoolManager.i.GetPool(NFT_ELEMENTS_POOL_NAME_PREFIX + name + GetInstanceID());
        if (pool != null) return pool;

        pool = PoolManager.i.AddPool(
            NFT_ELEMENTS_POOL_NAME_PREFIX + name + GetInstanceID(),
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
