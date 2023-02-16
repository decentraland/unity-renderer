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
    private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.75f, 0.75f, 0.75f);

    [SerializeField] private TMP_Text sectionName;
    [SerializeField] private ButtonComponentView backButton;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private UIPageSelector pageSelector;
    [SerializeField] private GameObject nftPageElement;

    public event Action OnBackFromViewAll;
    public event Action<string, int, int> OnRequestCollectibleElements;


    private List<PoolableObject> nftElementsPoolableQueue = new List<PoolableObject>();
    private Pool nftElementsEntryPool;
    private string section;

    public override void Start()
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>
        {
            ClearNftPool();
            OnBackFromViewAll?.Invoke();
        });
        pageSelector.OnValueChanged += RequestPage;
    }

    public void Initialize(int totalCollectiblesElements)
    {
        pageSelector.Setup(totalCollectiblesElements/ELEMENTS_PER_PAGE, false);
        nftElementsEntryPool = GetNftElementsEntryPool();
    }

    private void RequestPage(int pageNumber)
    {
        ClearNftPool();
        OnRequestCollectibleElements?.Invoke(section, pageNumber+1, ELEMENTS_PER_PAGE);
    }

    public override void RefreshControl()
    {
    }

    public void SetSectionName(string sectionNameText)
    {
        section = sectionNameText;
        sectionName.text = sectionNameText;
    }

    public void SetSectionQuantity(int totalCount)
    {
        sectionName.text = $"{sectionName.text} ({totalCount})";
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void ShowNftIcons(List<NFTIconComponentModel> models)
    {
        for (int i = 0; i < models.Count; i++)
        {
            PoolableObject poolableObject = nftElementsEntryPool.Get();
            nftElementsPoolableQueue.Add(poolableObject);
            poolableObject.gameObject.transform.SetParent(itemsContainer, false);
            poolableObject.gameObject.transform.localScale = NFT_ICON_SCALE;
            poolableObject.gameObject.GetComponent<NFTIconComponentView>().Configure(models[i]);
        }
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
        foreach (var poolObject in nftElementsPoolableQueue) { nftElementsEntryPool.Release(poolObject); }

        nftElementsPoolableQueue = new List<PoolableObject>();
    }
}
