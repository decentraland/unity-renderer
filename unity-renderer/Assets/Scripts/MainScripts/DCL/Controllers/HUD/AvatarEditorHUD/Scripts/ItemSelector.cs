using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DCL;
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
    [SerializeField] internal UIPageSelector pageSelector;
    [SerializeField] internal CollectionGroup collectionGroup;

    public event System.Action<string> OnItemClicked;
    public event System.Action<string> OnSellClicked;
    public event System.Action OnRetryClicked;

    internal Dictionary<string, ItemToggle> itemToggles = new Dictionary<string, ItemToggle>();
    internal Dictionary<string, WearableSettings> totalWearables = new Dictionary<string, WearableSettings>();
    internal List<WearableSettings> availableWearables = new List<WearableSettings>();
    internal List<string> selectedItems = new List<string>();

    private string currentBodyShape;
    private int maxWearables = 1;
    private int lastPage = 0;

    private void Awake()
    {
        Application.quitting += () =>
        {
            OnItemClicked = null;
        };

        pageSelector.OnValueChanged += UpdateWearableList;
    }

    private void CheckScreenSize(Vector2Int current, Vector2Int previous)
    {
        RectTransform rt = (RectTransform)transform;

        var width = Mathf.Max(current.x - 450, 200); // minus avatar margin;
        var height = Mathf.Max(current.y, 200);
        var aspectRatio = width / (float)height;
        
        // This is a lazy approach, every controlAspectRatio increases the collumns by one
        float controlAspectRatio = 0.15f;
        int rows = 3;

        var collumns = Mathf.FloorToInt(aspectRatio / controlAspectRatio);
        maxWearables = rows * collumns;

        SetupWearablePages();
    }

    private void OnEnable()
    {
        DataStore.i.screen.size.OnChange += CheckScreenSize;
        var currentScreenSize = DataStore.i.screen.size.Get();
        CheckScreenSize(currentScreenSize, currentScreenSize);
    }

    private void OnDisable()
    {
        DataStore.i.screen.size.OnChange -= CheckScreenSize;
    }

    private void SetupWearablePages()
    {
        collectionGroup.Setup(maxWearables);
        pageSelector.Setup(GetMaxSections());
        UpdateWearableList(lastPage);
    }

    private int GetMaxSections() => Mathf.CeilToInt(availableWearables.Count / (float)maxWearables);
    
    private void UpdateWearableList( int page )
    {
        lastPage = page;
        for (int itemToggleIndex = 0; itemToggleIndex < maxWearables; itemToggleIndex++)
        {
            var baseIndex = page * maxWearables;
            var wearableIndex = itemToggleIndex + baseIndex;

            if (wearableIndex < availableWearables.Count)
            {
                WearableSettings wearableSettings = availableWearables[wearableIndex];
                var item = wearableSettings.Item;
                var itemToggle = collectionGroup.LoadItem(itemToggleIndex, wearableSettings);
                itemToggle.SetCallbacks(ToggleClicked, SellClicked);
                itemToggle.SetLoadingSpinner(wearableSettings.isLoading);

                if (selectedItems.Contains(item.id))
                    itemToggle.selected = true;

                itemToggles[item.id] = itemToggle;
            }
            else
            {
                collectionGroup.HideItem(itemToggleIndex);
            }
        }
    }

    private void OnDestroy()
    {
        
    }

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
        
        WearableSettings wearableSettings = new WearableSettings(item, collectionName, amount, hideOtherWearablesToastStrategy, replaceOtherWearablesToastStrategy);
        totalWearables.Add(item.id, wearableSettings);

        if (item.SupportsBodyShape(currentBodyShape) || item.data.category == WearableLiterals.Categories.BODY_SHAPE)
        {
            availableWearables.Add(wearableSettings);
        }
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
        SetupWearablePages();
    }

    private void RefreshAvailableWearables()
    {
        availableWearables = totalWearables.Values.Where(w => w.Item.SupportsBodyShape(currentBodyShape)).ToList();
        SetupWearablePages();
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
        
    }

    public void ShowRetryLoading(bool isActive)
    {
        
    }

    private void RetryLoading() { OnRetryClicked?.Invoke(); }
}