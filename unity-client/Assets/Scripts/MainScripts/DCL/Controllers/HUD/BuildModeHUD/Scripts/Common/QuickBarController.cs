using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IQuickBarController
{
    event Action<int> OnQuickBarShortcutSelected;
    event Action<CatalogItem> OnCatalogItemSelected;

    void Initialize(IQuickBarView view, ISceneCatalogController sceneCatalogController);
    void Dispose();
    int GetSlotsCount();
    CatalogItem QuickBarObjectSelected(int index);
    void SetIndexToDrop(int index);
    void SceneObjectDroppedFromQuickBar(int fromQuickBarIndex, int toQuickBarIndex, Texture texture);
    void SceneObjectDroppedFromCatalog(BaseEventData data);
    void QuickBarInput(int quickBarSlot);
    void CancelDragging();
}

public class QuickBarController : IQuickBarController
{
    const int AMOUNT_OF_QUICK_SLOTS = 9;

    public event Action<int> OnQuickBarShortcutSelected;
    public event Action<CatalogItem> OnCatalogItemSelected;

    internal IQuickBarView quickBarView;
    internal ISceneCatalogController sceneCatalogController;

    internal CatalogItem[] quickBarShortcutsCatalogItems = new CatalogItem[AMOUNT_OF_QUICK_SLOTS];
    internal int lastIndexDroped = -1;

    public void Initialize(IQuickBarView quickBarView, ISceneCatalogController sceneCatalogController)
    {
        this.quickBarView = quickBarView;
        this.sceneCatalogController = sceneCatalogController;

        quickBarView.OnQuickBarObjectSelected += OnQuickBarObjectSelected;
        quickBarView.OnSetIndexToDrop += SetIndexToDrop;
        quickBarView.OnSceneObjectDroppedFromQuickBar += SceneObjectDroppedFromQuickBar;
        quickBarView.OnSceneObjectDroppedFromCatalog += SceneObjectDroppedFromCatalog;
        quickBarView.OnQuickBarInputTriggered += QuickBarInput;
        sceneCatalogController.OnStopInput += CancelDragging;
    }

    public void Dispose()
    {
        quickBarView.OnQuickBarObjectSelected -= OnQuickBarObjectSelected;
        quickBarView.OnSetIndexToDrop -= SetIndexToDrop;
        quickBarView.OnSceneObjectDroppedFromQuickBar -= SceneObjectDroppedFromQuickBar;
        quickBarView.OnSceneObjectDroppedFromCatalog -= SceneObjectDroppedFromCatalog;
        quickBarView.OnQuickBarInputTriggered -= QuickBarInput;
        sceneCatalogController.OnStopInput -= CancelDragging;
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
        RemoveQuickBarShortcut(fromQuickBarIndex);
    }

    public void SceneObjectDroppedFromCatalog(BaseEventData data)
    {
        CatalogItemAdapter adapter = sceneCatalogController.GetLastCatalogItemDragged();

        if (adapter != null &&
            adapter.thumbnailImg != null &&
            adapter.thumbnailImg.enabled)
        {
            Texture texture = adapter.thumbnailImg.texture;
            CatalogItem catalogItem = adapter.GetContent();
            SetQuickBarShortcut(catalogItem, lastIndexDroped, texture);
        }
    }

    private void RemoveQuickBarShortcut(int index)
    {
        quickBarShortcutsCatalogItems[index] = null;
        quickBarView.SetShortcutAsEmpty(index);
    }

    private void SetQuickBarShortcut(CatalogItem catalogItem, int index, Texture texture)
    {
        quickBarShortcutsCatalogItems[index] = catalogItem;
        quickBarView.SetTextureToShortcut(index, texture);
    }

    public void QuickBarInput(int quickBarSlot) { OnQuickBarShortcutSelected?.Invoke(quickBarSlot); }

    public void CancelDragging() { quickBarView.CancelCurrentDragging(); }
}