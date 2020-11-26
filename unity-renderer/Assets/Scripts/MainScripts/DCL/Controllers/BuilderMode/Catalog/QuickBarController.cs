using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickBarController : MonoBehaviour
{
    public RawImage[] shortcutsImgs;
    public Canvas generalCanvas;
    public CatalogGroupListView catalogGroupListView;

    public event System.Action OnResumeInput;
    public event System.Action OnStopInput;

    public event System.Action<SceneObject> OnSceneObjectSelected;

    List<SceneObject> quickBarShortcutsSceneObjects = new List<SceneObject>() { null, null, null, null, null, null, null, null, null };


    int lastIndexDroped = -1;
    GameObject draggedObject;


    private void Start()
    {
        catalogGroupListView.OnAdapterStartDragging += SceneObjectStartDragged;
        catalogGroupListView.OnAdapterDrag += OnDrag;
        catalogGroupListView.OnAdapterEndDrag += OnEndDrag;
    }

    private void OnDestroy()
    {
        catalogGroupListView.OnAdapterStartDragging -= SceneObjectStartDragged;
        catalogGroupListView.OnAdapterDrag -= OnDrag;
        catalogGroupListView.OnAdapterEndDrag -= OnEndDrag;
    }

    public void SetIndexToDrop(int index)
    {
        lastIndexDroped = index;
    }

    void OnDrag(PointerEventData data)
    {
        draggedObject.transform.position = data.position;
    }

    void SceneObjectStartDragged(SceneObject sceneObjectClicked, CatalogItemAdapter adapter, BaseEventData data)
    {
        PointerEventData eventData = data as PointerEventData;

        if (draggedObject == null)
            draggedObject = Instantiate(adapter.gameObject, generalCanvas.transform);

        CatalogItemAdapter newAdapter = draggedObject.GetComponent<CatalogItemAdapter>();

        RectTransform adapterRT = adapter.GetComponent<RectTransform>();
        newAdapter.SetContent(adapter.GetContent());
        newAdapter.EnableDragMode(adapterRT.sizeDelta);

        OnStopInput?.Invoke();
    }

    void OnEndDrag(PointerEventData data)
    {
        Destroy(draggedObject, 0.1f);
        OnResumeInput?.Invoke();
    }

    public void SceneObjectDropped(BaseEventData data)
    {

        CatalogItemAdapter adapter = draggedObject.GetComponent<CatalogItemAdapter>();
        SceneObject sceneObject = adapter.GetContent();
        Texture texture = null;
        if (adapter.thumbnailImg.enabled)
        {
            texture = adapter.thumbnailImg.texture;
            SetQuickBarShortcut(sceneObject, lastIndexDroped, texture);
        }
        Destroy(draggedObject);
    }

    public void QuickBarObjectSelected(int index)
    {
        if (quickBarShortcutsSceneObjects.Count > index && quickBarShortcutsSceneObjects[index] != null)
            OnSceneObjectSelected?.Invoke(quickBarShortcutsSceneObjects[index]);
    }

    void SetQuickBarShortcut(SceneObject sceneObject, int index, Texture texture)
    {
        quickBarShortcutsSceneObjects[index] = sceneObject;
        if (index < shortcutsImgs.Length)
        {
            shortcutsImgs[index].texture = texture;
            shortcutsImgs[index].enabled = true;
        }
    }

    int FindEmptyShortcutSlot()
    {
        int index = quickBarShortcutsSceneObjects.Count;
        int cont = 0;
        foreach (SceneObject sceneObjectIteration in quickBarShortcutsSceneObjects)
        {
            if (sceneObjectIteration == null)
            {
                index = cont;
                break;
            }
            cont++;
        }
        return index;
    }

}
