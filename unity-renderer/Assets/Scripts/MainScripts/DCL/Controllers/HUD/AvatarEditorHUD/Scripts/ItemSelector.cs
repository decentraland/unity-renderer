using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Configuration;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS4014

[assembly: InternalsVisibleTo("AvatarEditorHUDTests")]

public class ItemSelector : MonoBehaviour
{
    private const int MIN_SCREEN_SIZE = 200;
    private const int AVATAR_MARGIN = 450;
    private const float ASPECT_RATIO_PER_COLUMN = 0.15f;
    private const int TOTAL_ROWS_OF_ITEMS = 3;
    private const int MIN_AMOUNT_OF_COLUMNS = 3;

    [SerializeField] internal UIPageSelector pageSelector;
    [SerializeField] internal ItemToggleContainer itemToggleContainer;

    public event Action<string> OnItemClicked;
    public event Action<string> OnSellClicked;

    internal Dictionary<string, ItemToggle> currentItemToggles = new Dictionary<string, ItemToggle>();
    internal Dictionary<string, ItemToggle> newItemToggles = new Dictionary<string, ItemToggle>();
    internal readonly Dictionary<string, WearableSettings> totalWearables = new Dictionary<string, WearableSettings>();
    internal List<WearableSettings> availableWearables = new List<WearableSettings>();
    internal readonly List<string> selectedItems = new List<string>();

    private string currentBodyShape;
    private int maxVisibleWearables = 9;
    private int lastPage;
    
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private void Awake()
    {
        Application.quitting += () =>
        {
            OnItemClicked = null;
        };

        pageSelector.OnValueChanged += UpdateWearableList;
        DataStore.i.screen.size.OnChange += OnScreenSizeChanged;
    }

    private void OnEnable()
    {
        CheckScreenSize();
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    private void OnScreenSizeChanged(Vector2Int current, Vector2Int previous)
    {
        CheckScreenSize();
    }
    
    private void CheckScreenSize()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = new CancellationTokenSource();
        CheckScreenSizeAsync(cancellationTokenSource.Token);
    }

    private async UniTask CheckScreenSizeAsync(CancellationToken cancellationToken)
    {
        try {
            RectTransform rt = (RectTransform)transform;
            LayoutRebuilder.MarkLayoutForRebuild(rt);

            await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);
            
            cancellationToken.ThrowIfCancellationRequested();
        
            var rect = rt.rect;
            var width = Mathf.Max(rect.width, MIN_SCREEN_SIZE);
            float itemAndSpaceSize = 130 + 32f;
            var columns =  Mathf.Max(Mathf.CeilToInt(width / itemAndSpaceSize), MIN_AMOUNT_OF_COLUMNS);
            
            maxVisibleWearables = TOTAL_ROWS_OF_ITEMS * columns;

            SetupWearablePagination(true);
        }
        catch (OperationCanceledException) { }
    }

    private void SetupWearablePagination(bool forceRebuild = false)
    {
        if (!isActiveAndEnabled && !forceRebuild && !EnvironmentSettings.RUNNING_TESTS) return;
        itemToggleContainer.Setup(maxVisibleWearables);
        pageSelector.Setup(GetMaxPages(), forceRebuild);
        UpdateWearableList(lastPage);
    }

    private int GetMaxPages() => Mathf.CeilToInt(availableWearables.Count / (float)maxVisibleWearables);
    
    private void UpdateWearableList( int page )
    {
        lastPage = page;
        
        //we use this buffer to prevent items from being initialized with the same data twice after a screen resize or catalog refresh
        newItemToggles.Clear();
        
        for (int itemToggleIndex = 0; itemToggleIndex < maxVisibleWearables; itemToggleIndex++)
        {
            var baseIndex = page * maxVisibleWearables;
            var wearableIndex = itemToggleIndex + baseIndex;

            if (wearableIndex < availableWearables.Count)
            {
                WearableSettings wearableSettings = availableWearables[wearableIndex];
                var item = wearableSettings.Item;

                if (!currentItemToggles.ContainsKey(item.id))
                {
                    var itemToggle = itemToggleContainer.LoadItem(itemToggleIndex, wearableSettings);
                    itemToggle.SetCallbacks(ToggleClicked, SellClicked);
                    itemToggle.SetLoadingSpinner(wearableSettings.isLoading);
                    newItemToggles[item.id] = itemToggle;
                }
                else
                {
                    newItemToggles[item.id] = currentItemToggles[item.id];
                }
                
                if (selectedItems.Contains(item.id))
                    newItemToggles[item.id].selected = true;
            }
            else
            {
                itemToggleContainer.HideItem(itemToggleIndex);
            }
        }

        currentItemToggles = new Dictionary<string, ItemToggle>(newItemToggles);
    }

    public void AddWearable(
        WearableItem item,
        string collectionName,
        int amount,
        Func<WearableItem, bool> hideOtherWearablesToastStrategy,
        Func<WearableItem, bool> replaceOtherWearablesToastStrategy,
        Func<WearableItem, bool> incompatibleWearableToastStrategy)
    {
        if (item == null)
            return;
        
        if (totalWearables.ContainsKey(item.id))
            return;
        
        WearableSettings wearableSettings = new WearableSettings(item, collectionName, amount, hideOtherWearablesToastStrategy, replaceOtherWearablesToastStrategy, incompatibleWearableToastStrategy);
        totalWearables.Add(item.id, wearableSettings);

        availableWearables.Add(wearableSettings);
    }

    public void RemoveWearable(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return;

        totalWearables.Remove(itemID);
        currentItemToggles.Remove(itemID);

        RefreshAvailableWearables();
    }

    public void RemoveAllWearables()
    {
        totalWearables.Clear();
        availableWearables.Clear();
        currentItemToggles.Clear();

        UpdateSelectorLayout();
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
        SetupWearablePagination();
    }

    private void RefreshAvailableWearables()
    {
        availableWearables = totalWearables.Values.ToList();
        SetupWearablePagination();
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
        using (var iterator = currentItemToggles.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.selected = false;
            }
        }
    }

    internal void ToggleClicked(ItemToggle toggle) { OnItemClicked?.Invoke(toggle.wearableItem.id); }

    private void SellClicked(ItemToggle toggle) { OnSellClicked?.Invoke(toggle.wearableItem.id); }

    private ItemToggle GetItemToggleByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return null;
        return currentItemToggles.ContainsKey(itemID) ? currentItemToggles[itemID] : null;
    }
}