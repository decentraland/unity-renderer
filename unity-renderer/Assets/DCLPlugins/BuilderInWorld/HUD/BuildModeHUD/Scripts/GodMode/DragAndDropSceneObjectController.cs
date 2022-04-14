using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDragAndDropSceneObjectController
{
    event Action<CatalogItem> OnCatalogItemDropped;
    event Action OnDrop;
    event Action OnResumeInput;
    event Action OnStopInput;

    void Initialize(ISceneCatalogController catalogController, IDragAndDropSceneObjectView dragAndDropSceneObjectView);
    void Dispose();
    void Drop();
    CatalogItemAdapter GetLastAdapterDragged();
}

public class DragAndDropSceneObjectController : IDragAndDropSceneObjectController
{
    public event Action OnResumeInput;
    public event Action OnStopInput;
    public event Action<CatalogItem> OnCatalogItemDropped;
    public event Action OnDrop;

    private ISceneCatalogController sceneCatalogController;

    private CatalogItemAdapter catalogItemAdapterDragged;
    internal CatalogItemAdapter catalogItemCopy;
    internal CatalogItem itemDroped;
    private IDragAndDropSceneObjectView dragAndDropSceneObjectView;

    public void Initialize(ISceneCatalogController sceneCatalogController, IDragAndDropSceneObjectView dragAndDropSceneObjectView)
    {
        this.sceneCatalogController = sceneCatalogController;
        sceneCatalogController.OnCatalogItemStartDrag += AdapterStartDragging;

        this.dragAndDropSceneObjectView = dragAndDropSceneObjectView;
        this.dragAndDropSceneObjectView.OnDrop += Drop;
    }

    public void Dispose()
    {
        if(sceneCatalogController != null)
            sceneCatalogController.OnCatalogItemStartDrag -= AdapterStartDragging;
        
        if(dragAndDropSceneObjectView != null)
            dragAndDropSceneObjectView.OnDrop -= Drop;
        
        if (catalogItemCopy != null && catalogItemCopy.gameObject != null )
            GameObject.Destroy(catalogItemCopy.gameObject);
    }

    public void Drop()
    {
        CatalogItemDropped();
        OnDrop?.Invoke();
    }

    public void CatalogItemDropped()
    {
        if (catalogItemCopy == null)
            return;

        // If an item has been dropped in the view , we assign it as itemDropped and wait for the OnEndDrag to process the item
        CatalogItem catalogItem = catalogItemAdapterDragged.GetContent();
        itemDroped = catalogItem;
    }

    public CatalogItemAdapter GetLastAdapterDragged() { return catalogItemAdapterDragged; }

    internal void AdapterStartDragging(CatalogItem catalogItemClicked, CatalogItemAdapter adapter)
    {
        // We create a copy of the adapter that has been dragging to move with the mouse as feedback
        var catalogItemAdapterDraggedGameObject = GameObject.Instantiate(adapter.gameObject, dragAndDropSceneObjectView.GetGeneralCanvas().transform);
        catalogItemCopy = catalogItemAdapterDraggedGameObject.GetComponent<CatalogItemAdapter>();

        RectTransform adapterRT = adapter.GetComponent<RectTransform>();
        catalogItemCopy.SetContent(adapter.GetContent());
        catalogItemCopy.EnableDragMode(adapterRT.sizeDelta);

        // However, since we have starting the drag event in the original adapter,
        // We need to track the drag event in the original and apply the event to the copy 
        adapter.OnAdapterDrag += OnDrag;
        adapter.OnAdapterEndDrag += OnEndDrag;
        catalogItemAdapterDragged = adapter;
        OnStopInput?.Invoke();
    }

    internal void OnDrag(PointerEventData data) {  MoveCopyAdapterToPosition(data.position); }

    internal void MoveCopyAdapterToPosition(Vector3 position) { catalogItemCopy.gameObject.transform.position = position; }

    internal void OnEndDrag(PointerEventData data)
    {
        OnResumeInput?.Invoke();
        if (catalogItemAdapterDragged != null)
        {
            catalogItemAdapterDragged.OnAdapterDrag -= OnDrag;
            catalogItemAdapterDragged.OnAdapterEndDrag -= OnEndDrag;
        }
        GameObject.Destroy(catalogItemCopy.gameObject);

        // Note(Adrian): If a item has been dropped in the "drop view" we process it here since this event complete the drag and drop flow
        // If we don't wait for the full flow to finish, the OnCatalogItemDropped could refresh the catalog breaking the references
        if (itemDroped != null)
        {
            OnCatalogItemDropped?.Invoke(itemDroped);
            itemDroped = null;
        }
    }
}