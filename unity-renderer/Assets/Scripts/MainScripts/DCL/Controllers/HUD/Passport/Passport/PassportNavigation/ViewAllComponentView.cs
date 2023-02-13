using System;
using DCL;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ViewAllComponentView : BaseComponentView, IViewAllComponentView
{
    private const int ELEMENTS_PER_PAGE = 20;
    private const string NFT_ELEMENTS_POOL_NAME_PREFIX = "NFTElementsEntriesPool_";

    [SerializeField] private TMP_Text sectionName;
    [SerializeField] private ButtonComponentView backButton;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private UIPageSelector pageSelector;
    [SerializeField] private GameObject nftPageElement;

    public event Action OnBackFromViewAll;
    public event Action<int, int> OnRequestCollectibleElements;


    private List<PoolableObject> nftElementsPoolableQueue = new List<PoolableObject>();
    private Pool nftElementsEntryPool;

    public override void Start()
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackFromViewAll?.Invoke());
        pageSelector.OnValueChanged += RequestPage;
    }

    public void Initialize(int totalCollectiblesElements)
    {
        pageSelector.Setup(totalCollectiblesElements/ELEMENTS_PER_PAGE, false);
        nftElementsEntryPool = GetNftElementsEntryPool();
    }

    private void RequestPage(int pageNumber)
    {
        OnRequestCollectibleElements?.Invoke(pageNumber, ELEMENTS_PER_PAGE);
    }

    public override void RefreshControl()
    {
    }

    public void SetSectionName(string sectionNameText)
    {
        sectionName.text = sectionNameText;
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
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
}
