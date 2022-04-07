using DCL;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IQuickBarController
{
    event Action<int> OnQuickBarShortcutSelected;
    event Action<CatalogItem> OnCatalogItemSelected;
    event Action<CatalogItem> OnCatalogItemAssigned;

    void Initialize(IQuickBarView view, IDragAndDropSceneObjectController dragAndDropSceneObjectController);
    void Dispose();
    int GetSlotsCount();
    CatalogItem QuickBarObjectSelected(int index);
    void SetIndexToDrop(int index);
    void SceneObjectDroppedFromQuickBar(int fromQuickBarIndex, int toQuickBarIndex, Texture texture);
    void SceneObjectDroppedFromCatalog(BaseEventData data);
    void SetQuickBarShortcut(CatalogItem catalogItem, int index, Texture texture);
    void QuickBarInput(int quickBarSlot);
    void CancelDragging();
}

public class QuickBarController : IQuickBarController
{
    const int AMOUNT_OF_QUICK_SLOTS = 9;

    public event Action<int> OnQuickBarShortcutSelected;
    public event Action<CatalogItem> OnCatalogItemSelected;
    public event Action<CatalogItem> OnCatalogItemAssigned;

    internal IQuickBarView quickBarView;
    internal IDragAndDropSceneObjectController dragAndDropController;

    internal CatalogItem[] quickBarShortcutsCatalogItems = new CatalogItem[AMOUNT_OF_QUICK_SLOTS];
    internal AssetPromise_Texture[] quickBarShortcutsThumbnailPromises = new AssetPromise_Texture[AMOUNT_OF_QUICK_SLOTS];
    internal int lastIndexDroped = -1;

    public void Initialize(IQuickBarView quickBarView, IDragAndDropSceneObjectController dragAndDropController)
    {
        this.quickBarView = quickBarView;
        this.dragAndDropController = dragAndDropController;

        quickBarView.OnQuickBarObjectSelected += OnQuickBarObjectSelected;
        quickBarView.OnSetIndexToDrop += SetIndexToDrop;
        quickBarView.OnSceneObjectDroppedFromQuickBar += SceneObjectDroppedFromQuickBar;
        quickBarView.OnSceneObjectDroppedFromCatalog += SceneObjectDroppedFromCatalog;
        quickBarView.OnQuickBarInputTriggered += QuickBarInput;
        dragAndDropController.OnStopInput += CancelDragging;
    }

    public void Dispose()
    {
        if (quickBarView != null)
        {
            quickBarView.OnQuickBarObjectSelected -= OnQuickBarObjectSelected;
            quickBarView.OnSetIndexToDrop -= SetIndexToDrop;
            quickBarView.OnSceneObjectDroppedFromQuickBar -= SceneObjectDroppedFromQuickBar;
            quickBarView.OnSceneObjectDroppedFromCatalog -= SceneObjectDroppedFromCatalog;
            quickBarView.OnQuickBarInputTriggered -= QuickBarInput;
        }
        if(dragAndDropController != null)
            dragAndDropController.OnStopInput -= CancelDragging;

        foreach (AssetPromise_Texture loadedThumbnailPromise in quickBarShortcutsThumbnailPromises)
        {
            if (loadedThumbnailPromise == null)
                continue;

            ClearThumbnailPromise(loadedThumbnailPromise);
        }
    }

    public int GetSlotsCount() { return AMOUNT_OF_QUICK_SLOTS; }

    public CatalogItem QuickBarObjectSelected(int index)
    {
        if (quickBarShortcutsCatalogItems.Length > index && quickBarShortcutsCatalogItems[index] != null)
        {
            OnCatalogItemSelected?.Invoke(quickBarShortcutsCatalogItems[index]);
            return quickBarShortcutsCatalogItems[index];
        }

        return null;
    }

    private void OnQuickBarObjectSelected(int obj) { QuickBarObjectSelected(obj); }

    public void SetIndexToDrop(int index) { lastIndexDroped = index; }

    public void SceneObjectDroppedFromQuickBar(int fromQuickBarIndex, int toQuickBarIndex, Texture texture)
    {
        SetQuickBarShortcut(quickBarShortcutsCatalogItems[fromQuickBarIndex], toQuickBarIndex, texture);
        MoveQuickbarThumbnailPromise(fromQuickBarIndex, toQuickBarIndex);
        RemoveQuickBarShortcut(fromQuickBarIndex);
    }

    private void MoveQuickbarThumbnailPromise(int fromQuickBarIndex, int toQuickBarIndex)
    {
        quickBarShortcutsThumbnailPromises[toQuickBarIndex] = quickBarShortcutsThumbnailPromises[fromQuickBarIndex];
        quickBarShortcutsThumbnailPromises[fromQuickBarIndex] = null;
    }

    public void SceneObjectDroppedFromCatalog(BaseEventData data)
    {
        CatalogItemAdapter adapter = dragAndDropController.GetLastAdapterDragged();

        if (adapter != null)
            SetCatalogItemToShortcut(adapter.GetContent());
    }

    private void SetCatalogItemToShortcut(CatalogItem catalogItem)
    {
        if (catalogItem == null)
            return;

        var url = catalogItem.GetThumbnailUrl();

        if (string.IsNullOrEmpty(url))
            return;

        ClearThumbnailPromise(quickBarShortcutsThumbnailPromises[lastIndexDroped]);
        quickBarShortcutsThumbnailPromises[lastIndexDroped] = new AssetPromise_Texture(url);

        quickBarShortcutsThumbnailPromises[lastIndexDroped].OnSuccessEvent += x =>
        {
            SetQuickBarShortcut(catalogItem, lastIndexDroped, x.texture);
        };

        quickBarShortcutsThumbnailPromises[lastIndexDroped].OnFailEvent += (x, error) =>
        {
            Debug.Log($"Error downloading: {url}, Exception: {error}");
        };

        AssetPromiseKeeper_Texture.i.Keep(quickBarShortcutsThumbnailPromises[lastIndexDroped]);
    }

    private void ClearThumbnailPromise(AssetPromise_Texture thumbnailToClear)
    {
        if (thumbnailToClear != null)
        {
            thumbnailToClear.ClearEvents();
            AssetPromiseKeeper_Texture.i.Forget(thumbnailToClear);
            thumbnailToClear = null;
        }
    }

    private void RemoveQuickBarShortcut(int index)
    {
        quickBarShortcutsCatalogItems[index] = null;
        quickBarView.SetShortcutAsEmpty(index);
        ClearThumbnailPromise(quickBarShortcutsThumbnailPromises[index]);
    }

    public void SetQuickBarShortcut(CatalogItem catalogItem, int index, Texture texture)
    {
        quickBarShortcutsCatalogItems[index] = catalogItem;
        quickBarView.SetTextureToShortcut(index, texture);
        OnCatalogItemAssigned?.Invoke(catalogItem);
    }

    public void QuickBarInput(int quickBarSlot) { OnQuickBarShortcutSelected?.Invoke(quickBarSlot); }

    public void CancelDragging() { quickBarView.CancelCurrentDragging(); }
}