using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[assembly: InternalsVisibleTo("AvatarEditorHUDTests")]

public class WearableSettings
{
    public WearableItem Item { get; }
    public string CollectionName { get; }
    public int Amount { get; }
    
    public bool isLoading { get; set; }
    public Func<WearableItem, bool> HideOtherWearablesToastStrategy { get; }
    public Func<WearableItem, bool> ReplaceOtherWearablesToastStrategy { get; }
    public WearableSettings(WearableItem item, string collectionName, int amount, Func<WearableItem, bool> hideOtherWearablesToastStrategy, Func<WearableItem, bool> replaceOtherWearablesToastStrategy)
    {
        Item = item;
        CollectionName = collectionName;
        Amount = amount;
        HideOtherWearablesToastStrategy = hideOtherWearablesToastStrategy;
        ReplaceOtherWearablesToastStrategy = replaceOtherWearablesToastStrategy;
    }

    public override string ToString() { return Item.id; }
}

public class ItemSelector : MonoBehaviour
{
    private const string DECENTRALAND_COLLECTION_ID = "Decentraland";

    [SerializeField]
    internal NFTSkinFactory nftSkinFactory;

    [SerializeField]
    internal CollectionGroup collectionGroupPrefab;

    [SerializeField]
    internal RectTransform content;

    [SerializeField]
    internal GameObject loadingSpinner;

    [SerializeField]
    internal GameObject loadingRetry;

    [SerializeField]
    internal Button loadingRetryButton;
    
    [SerializeField]
    internal GameObject wearablePagesRoot;
    
    [SerializeField]
    internal Button nextWearablePage;
    
    [SerializeField]
    internal Button previousWearablePage;
    
    [SerializeField]
    internal TMP_Text wearablePagesText;

    public event System.Action<string> OnItemClicked;
    public event System.Action<string> OnSellClicked;
    public event System.Action OnRetryClicked;

    internal Dictionary<string, ItemToggle> itemToggles = new Dictionary<string, ItemToggle>();
    internal Dictionary<string, CollectionGroup> currentCollectionGroups = new Dictionary<string, CollectionGroup>();
    internal List<WearableSettings> availableWearables = new List<WearableSettings>();
    internal Dictionary<string, WearableSettings> totalWearables = new Dictionary<string, WearableSettings>();
    internal List<string> selectedItems = new List<string>();

    private string currentBodyShape;

    private void Awake()
    {
        Application.quitting += () =>
        {
            OnItemClicked = null;
        };

        loadingRetryButton.onClick.AddListener(RetryLoading);
        nextWearablePage.onClick.AddListener(NextPage);
        previousWearablePage.onClick.AddListener(PreviousPage);
    }
    private void NextPage()
    {
        currentSection = (currentSection + 1 ) % GetMaxSections();
        UpdateWearableList();
    }
    private void PreviousPage()
    {
        currentSection = (currentSection - 1 );

        if (currentSection < 0)
            currentSection = GetMaxSections();

        currentSection %= GetMaxSections();
        
        UpdateWearableList();
    }


    private const float MAX_WEARABLES = 21;
    private int GetMaxSections() => Mathf.CeilToInt(availableWearables.Count / MAX_WEARABLES);
    private int currentSection = 0;

    private void OnEnable() { UpdateWearableList(); }
    private void UpdateWearableList()
    {
        int maxSections = GetMaxSections();
        
        wearablePagesRoot.SetActive(maxSections > 1);
        wearablePagesText.text = $"{currentSection+1}/{maxSections}";
        
        // TODO: Fix different collections
        CollectionGroup collection = null;
        
        for (int itemToggleIndex = 0; itemToggleIndex < MAX_WEARABLES; itemToggleIndex++)
        {
            var baseIndex = currentSection * (int)MAX_WEARABLES;
            var wearableIndex = itemToggleIndex + baseIndex;
            
            if (wearableIndex < availableWearables.Count)
            {
                WearableSettings wearableSettings = availableWearables[wearableIndex];
                var item = wearableSettings.Item;
                var collectionName = wearableSettings.CollectionName;
                collection = GetCollectionGroupFor(item, collectionName);
                var itemToggle = collection.LoadItem(itemToggleIndex, wearableSettings, collection.collectionId);
                itemToggle.SetCallbacks(ToggleClicked, SellClicked);
                itemToggle.SetLoadingSpinner(wearableSettings.isLoading);

                if (selectedItems.Contains(item.id))
                    itemToggle.selected = true;
                
                itemToggles[item.id] = itemToggle;
            }
            else
            {
                if (collection != null)
                {
                    collection.HideItem(itemToggleIndex);
                }
            }
        }
    }

    private void OnDestroy() { loadingRetryButton.onClick.RemoveListener(RetryLoading); }

    public void AddItemToggle(
        WearableItem item,
        string collectionName,
        int amount,
        Func<WearableItem, bool> hideOtherWearablesToastStrategy,
        Func<WearableItem, bool> replaceOtherWearablesToastStrategy)
    {
        if (item == null)
            return;
        
        if (totalWearables.ContainsKey(item.id))
            return;

        var collectionGroup = GetCollectionGroupFor(item, collectionName);
        
        WearableSettings wearableSettings = new WearableSettings(item, collectionName, amount, hideOtherWearablesToastStrategy, replaceOtherWearablesToastStrategy);
        totalWearables.Add(item.id, wearableSettings);

        if (item.SupportsBodyShape(currentBodyShape) || item.data.category == WearableLiterals.Categories.BODY_SHAPE)
        {
            availableWearables.Add(wearableSettings);
        }
    }
    private CollectionGroup GetCollectionGroupFor(WearableItem item, string collectionName)
    {
        CollectionGroup collectionGroup;

        if (item.IsFromThirdPartyCollection)
            collectionGroup = CreateCollectionGroupIfNeeded(item.ThirdPartyCollectionId, collectionName);
        else
            collectionGroup = CreateCollectionGroupIfNeeded(DECENTRALAND_COLLECTION_ID, DECENTRALAND_COLLECTION_ID);

        return collectionGroup;
    }

    public void RemoveItemToggle(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return;

        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle == null)
            return;

        itemToggles.Remove(itemID);
        Destroy(toggle.gameObject);
        RemoveCollectionGroupIfNeeded(toggle.collectionId);
    }

    public void RemoveAllItemToggle()
    {
        // We dont really want to do this
        /*using (var it = itemToggles.GetEnumerator())
        {
            while (it.MoveNext())
            {
                Destroy(it.Current.Value.gameObject);
                RemoveCollectionGroupIfNeeded(it.Current.Value.collectionId);
            }
        }

        itemToggles.Clear();*/
    }

    public void SetBodyShape(string bodyShape)
    {
        if (currentBodyShape == bodyShape)
            return;

        currentBodyShape = bodyShape;
        RefreshAvailableWearables();
    }

    public void UpdateSelectorLayout()
    {
        UpdateWearableList();
    }

    private void RefreshAvailableWearables()
    {
        availableWearables = totalWearables.Values.Where(w => w.Item.SupportsBodyShape(currentBodyShape)).ToList();
        UpdateWearableList();
    }

    public void Select(string itemID)
    {
        selectedItems.Add(itemID);
        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle != null)
            toggle.selected = true;
    }

    public void SetWearableLoadingSpinner(string wearableID, bool isActive)
    {
        if (totalWearables.ContainsKey(wearableID))
        {
            totalWearables[wearableID].isLoading = isActive;
        }
        
        ItemToggle toggle = GetItemToggleByID(wearableID);
        if (toggle != null)
            toggle.SetLoadingSpinner(isActive);
    }

    public void Unselect(string itemID)
    {
        selectedItems.Remove(itemID);
        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle != null)
            toggle.selected = false;
    }

    public void UnselectAll()
    {
        selectedItems.Clear();
        using (var iterator = itemToggles.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.selected = false;
            }
        }
    }

    private void ToggleClicked(ItemToggle toggle) { OnItemClicked?.Invoke(toggle.wearableItem.id); }

    private void SellClicked(ItemToggle toggle) { OnSellClicked?.Invoke(toggle.wearableItem.id); }

    private ItemToggle GetItemToggleByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return null;
        return itemToggles.ContainsKey(itemID) ? itemToggles[itemID] : null;
    }

    public void ShowLoading(bool isActive)
    {
        loadingSpinner.SetActive(isActive);
        loadingSpinner.transform.SetAsLastSibling();
    }

    public void ShowRetryLoading(bool isActive)
    {
        loadingRetry.SetActive(isActive);
        loadingRetry.transform.SetAsLastSibling();
    }

    private void RetryLoading() { OnRetryClicked?.Invoke(); }

    private CollectionGroup CreateCollectionGroupIfNeeded(string collectionId, string collectionName)
    {
        if (currentCollectionGroups.ContainsKey(collectionId))
            return currentCollectionGroups[collectionId];

        CollectionGroup newCollectionGroup = Instantiate(collectionGroupPrefab, content);
        newCollectionGroup.Configure(collectionId, collectionName);
        currentCollectionGroups.Add(collectionId, newCollectionGroup);

        if (collectionId == DECENTRALAND_COLLECTION_ID)
            newCollectionGroup.transform.SetAsFirstSibling();
        else
            newCollectionGroup.transform.SetAsLastSibling();

        return newCollectionGroup;
    }

    private bool RemoveCollectionGroupIfNeeded(string collectionId)
    {
        currentCollectionGroups.TryGetValue(collectionId, out CollectionGroup collectionGroupToRemove);
        if (collectionGroupToRemove != null && itemToggles.Count(x => x.Value.collectionId == collectionId) == 0)
        {
            currentCollectionGroups.Remove(collectionId);
            Destroy(collectionGroupToRemove.gameObject);
            return true;
        }

        return false;
    }
}