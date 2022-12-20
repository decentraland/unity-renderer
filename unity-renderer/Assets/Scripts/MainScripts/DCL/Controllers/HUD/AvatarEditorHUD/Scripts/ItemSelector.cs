using Cysharp.Threading.Tasks;
using DCL;
using DCL.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("AvatarEditorHUDTests")]

public class ItemSelector : MonoBehaviour
{
    private const int MIN_SCREEN_SIZE = 200;
    private const int TOTAL_ROWS_OF_ITEMS = 3;
    private const int MIN_AMOUNT_OF_COLUMNS = 3;

    [SerializeField] internal UIPageSelector pageSelector;
    [SerializeField] internal ItemToggleContainer itemToggleContainer;

    public event Action<string> OnItemClicked;
    public event Action<string> OnSellClicked;

    private readonly Dictionary<string, ItemToggle> newItemToggles = new ();
    internal readonly Dictionary<string, WearableSettings> totalWearables = new ();
    private readonly List<string> selectedItems = new ();

    internal Dictionary<string, ItemToggle> currentItemToggles = new ();
    private List<WearableSettings> availableWearables = new ();

    private string currentBodyShape;
    private int maxVisibleWearables = 9;
    private int lastPage;

    private CancellationTokenSource cancellationTokenSource = new ();

    private void Awake()
    {
        Application.quitting += () => { OnItemClicked = null; };

        pageSelector.OnValueChanged += UpdateWearableList;
        DataStore.i.screen.size.OnChange += OnScreenSizeChanged;
    }

    private void OnEnable()
    {
        SetupPaginationWithColumns();
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    private void OnScreenSizeChanged(Vector2Int _, Vector2Int __) =>
        SetupPaginationWithColumns();

    private void SetupPaginationWithColumns()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = new CancellationTokenSource();

        SetupPaginationWithColumnsAsync(cancellationTokenSource.Token).Forget();
    }

    private async UniTask SetupPaginationWithColumnsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var rt = (RectTransform)transform;
            LayoutRebuilder.MarkLayoutForRebuild(rt);

            await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            maxVisibleWearables = TOTAL_ROWS_OF_ITEMS * CalculateColumnsAmount(rt.rect);
            await SetupWearablePagination(cancellationToken, true);
        }
        catch (OperationCanceledException) { }
    }

    private static int CalculateColumnsAmount(Rect rect)
    {
        float width = Mathf.Max(rect.width, MIN_SCREEN_SIZE);
        float itemAndSpaceSize = 130 + 32f;

        return Mathf.Max(Mathf.CeilToInt(width / itemAndSpaceSize), MIN_AMOUNT_OF_COLUMNS);
    }

    private async UniTask SetupWearablePagination(CancellationToken token, bool forceRebuild = false)
    {
        if (isActiveAndEnabled || forceRebuild || EnvironmentSettings.RUNNING_TESTS)
        {
            itemToggleContainer.Rebuild(maxVisibleWearables);
            pageSelector.Setup(GetMaxPages(), forceRebuild);

            await UpdateWearableListAsync(lastPage, token);
        }
    }

    private int GetMaxPages() =>
        Mathf.CeilToInt(availableWearables.Count / (float)maxVisibleWearables);

    private void UpdateWearableList(int page)
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = new CancellationTokenSource();

        UpdateWearableListAsync(page, cancellationTokenSource.Token).Forget();
    }

    private async UniTask UpdateWearableListAsync(int page, CancellationToken token)
    {
        lastPage = page;

        // we use this buffer to prevent items from being initialized with the same data twice after a screen resize or catalog refresh
        newItemToggles.Clear();

        int baseIndex = page * maxVisibleWearables;

        for (var itemToggleIndex = 0; itemToggleIndex < maxVisibleWearables; itemToggleIndex++)
        {
            int wearableIndex = baseIndex + itemToggleIndex;

            if (wearableIndex < availableWearables.Count)
                await ShowItem(availableWearables[wearableIndex], itemToggleIndex, token);
            else
                itemToggleContainer.HideItem(itemToggleIndex);
        }

        currentItemToggles = new Dictionary<string, ItemToggle>(newItemToggles);
    }

    private async Task ShowItem(WearableSettings wearableSettings, int itemToggleIndex, CancellationToken token)
    {
        WearableItem item = wearableSettings.Item;

        if (currentItemToggles.ContainsKey(item.id))
            newItemToggles[item.id] = currentItemToggles[item.id];
        else
        {
            ItemToggle itemToggle = await itemToggleContainer.LoadItemAsync(itemToggleIndex, wearableSettings);

            itemToggle.SetCallbacks(ToggleClicked, SellClicked);
            itemToggle.SetLoadingSpinner(wearableSettings.isLoading);

            newItemToggles[item.id] = itemToggle;

            await UniTask.Yield(token);
        }

        if (selectedItems.Contains(item.id))
            newItemToggles[item.id].selected = true;
    }

    public void AddWearable(WearableItem item, string collectionName, int amount,
        Func<WearableItem, bool> hideOtherWearablesToastStrategy,
        Func<WearableItem, bool> replaceOtherWearablesToastStrategy,
        Func<WearableItem, bool> incompatibleWearableToastStrategy)
    {
        if (item == null)
            return;

        if (totalWearables.ContainsKey(item.id))
            return;

        var wearableSettings = new WearableSettings(item, collectionName, amount, hideOtherWearablesToastStrategy, replaceOtherWearablesToastStrategy, incompatibleWearableToastStrategy);
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
        SetupWearablePagination(CancellationToken.None).Forget();
    }

    private void RefreshAvailableWearables()
    {
        availableWearables = totalWearables.Values.ToList();
        SetupWearablePagination(CancellationToken.None).Forget();
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
            totalWearables[wearableID].isLoading = isActive;

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
        using Dictionary<string, ItemToggle>.Enumerator iterator = currentItemToggles.GetEnumerator();

        while (iterator.MoveNext())
            iterator.Current.Value.selected = false;
    }

    private void ToggleClicked(ItemToggle toggle) =>
        OnItemClicked?.Invoke(toggle.wearableItem.id);

    private void SellClicked(ItemToggle toggle) =>
        OnSellClicked?.Invoke(toggle.wearableItem.id);

    private ItemToggle GetItemToggleByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return null;

        return currentItemToggles.ContainsKey(itemID) ? currentItemToggles[itemID] : null;
    }
}
