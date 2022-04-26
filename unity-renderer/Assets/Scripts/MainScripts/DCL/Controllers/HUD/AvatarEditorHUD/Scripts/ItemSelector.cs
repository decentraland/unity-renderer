using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("AvatarEditorHUDTests")]

public class ItemSelector : MonoBehaviour
{
    private const string DECENTRALAND_COLLECTION_ID = "Decentraland";

    [SerializeField]
    internal ItemToggleFactory itemToggleFactory;

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

    public event System.Action<string> OnItemClicked;
    public event System.Action<string> OnSellClicked;
    public event System.Action OnRetryClicked;

    internal Dictionary<string, ItemToggle> itemToggles = new Dictionary<string, ItemToggle>();
    internal Dictionary<string, CollectionGroup> currentCollectionGroups = new Dictionary<string, CollectionGroup>();

    private string currentBodyShape;

    private void Awake()
    {
        Application.quitting += () =>
        {
            OnItemClicked = null;
        };

        loadingRetryButton.onClick.AddListener(RetryLoading);
    }

    private void OnDestroy() { loadingRetryButton.onClick.RemoveListener(RetryLoading); }

    public void AddItemToggle(
        WearableItem item,
        string collectionName,
        int amount,
        Func<WearableItem, bool> hideOtherWearablesToastStrategy,
        Func<WearableItem, bool> replaceOtherWearablesToastStrategy)
    {
        CollectionGroup collectionGroup;
        if (item.IsFromThirdPartyCollection)
            collectionGroup = CreateCollectionGroupIfNeeded(item.ThirdPartyCollectionId, collectionName);
        else
            collectionGroup = CreateCollectionGroupIfNeeded(DECENTRALAND_COLLECTION_ID, DECENTRALAND_COLLECTION_ID);

        if (item == null)
            return;
        if (itemToggles.ContainsKey(item.id))
            return;

        ItemToggle newToggle;
        if (item.IsCollectible())
        {
            newToggle = itemToggleFactory.CreateItemToggleFromRarity(item.rarity, collectionGroup.itemContainer);
            newToggle.transform.SetAsFirstSibling();
        }
        else
        {
            newToggle = itemToggleFactory.CreateBaseWearable(collectionGroup.itemContainer);
        }

        newToggle.Initialize(item, false, amount);
        newToggle.SetHideOtherWerablesToastStrategy(hideOtherWearablesToastStrategy);
        newToggle.SetReplaceOtherWearablesToastStrategy(replaceOtherWearablesToastStrategy);
        newToggle.OnClicked += ToggleClicked;
        newToggle.OnSellClicked += SellClicked;
        newToggle.collectionId = collectionGroup.collectionId;
        itemToggles.Add(item.id, newToggle);

        bool active = string.IsNullOrEmpty(currentBodyShape) || item.SupportsBodyShape(currentBodyShape);
        newToggle.gameObject.SetActive(active);
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
        using (var it = itemToggles.GetEnumerator())
        {
            while (it.MoveNext())
            {
                Destroy(it.Current.Value.gameObject);
                RemoveCollectionGroupIfNeeded(it.Current.Value.collectionId);
            }
        }

        itemToggles.Clear();
    }

    public void SetBodyShape(string bodyShape)
    {
        if (currentBodyShape == bodyShape)
            return;

        currentBodyShape = bodyShape;
        ShowCompatibleWithBodyShape();
    }

    public void UpdateSelectorLayout() { Utils.ForceUpdateLayout(content); }

    private void ShowCompatibleWithBodyShape()
    {
        using (Dictionary<string, ItemToggle>.Enumerator iterator = itemToggles.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                ItemToggle current = iterator.Current.Value;
                bool active = current.wearableItem.SupportsBodyShape(currentBodyShape);
                current.gameObject.SetActive(active);
            }
        }
    }

    public void Select(string itemID)
    {
        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle != null)
            toggle.selected = true;
    }

    public void SetWearableLoadingSpinner(string wearableID, bool isActive)
    {
        ItemToggle toggle = GetItemToggleByID(wearableID);
        if (toggle != null)
            toggle.SetLoadingSpinner(isActive);
    }

    public void Unselect(string itemID)
    {
        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle != null)
            toggle.selected = false;
    }

    public void UnselectAll()
    {
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