using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IGridContainerComponentView
{
    /// <summary>
    /// Fill the model and updates the grid with this data.
    /// </summary>
    /// <param name="model">Data to configure the grid.</param>
    void Configure(GridContainerComponentModel model);

    /// <summary>
    /// Set the number of columns of the grid.
    /// </summary>
    /// <param name="newNumColumns">Number of columns.</param>
    void SetNumColumns(int newNumColumns);

    /// <summary>
    /// Set the size of each child item.
    /// </summary>
    /// <param name="newItemSize">Size of each child.</param>
    void SetItemSize(Vector2Int newItemSize);

    /// <summary>
    /// Set the space between child items.
    /// </summary>
    /// <param name="newSpace">Space between children.</param>
    void SetSpaceBetweenItems(Vector2Int newSpace);

    /// <summary>
    /// Set the items of the grid.
    /// </summary>
    /// <param name="items">List of UI components.</param>
    void SetItems(List<BaseComponentView> items);

    /// <summary>
    /// Get an item of the grid.
    /// </summary>
    /// <param name="index">Index of the list of items.</param>
    /// <returns>A specific UI component.</returns>
    BaseComponentView GetItem(int index);
}

public class GridContainerComponentView : BaseComponentView, IGridContainerComponentView
{
    [Header("Prefab References")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("Configuration")]
    [SerializeField] protected GridContainerComponentModel model;

    private List<BaseComponentView> currentItems = new List<BaseComponentView>();

    public void Configure(GridContainerComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        SetNumColumns(model.numColumns);
        SetItemSize(model.itemSize);
        SetSpaceBetweenItems(model.spaceBetweensItems);
        SetItems(model.items);
    }

    public void SetNumColumns(int newNumColumns)
    {
        model.numColumns = newNumColumns;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.constraintCount = newNumColumns;
    }

    public void SetItemSize(Vector2Int newItemSize)
    {
        model.itemSize = newItemSize;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.cellSize = newItemSize;
    }

    public void SetSpaceBetweenItems(Vector2Int newSpace)
    {
        model.spaceBetweensItems = newSpace;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.spacing = newSpace;
    }

    public void SetItems(List<BaseComponentView> items)
    {
        model.items = items;

        RemoveAllIntantiatedItems();

        for (int i = 0; i < items.Count; i++)
        {
            InstantiateItem(items[i], $"Item{i}");
        }
    }

    public BaseComponentView GetItem(int index)
    {
        if (index >= currentItems.Count)
            return null;

        return currentItems[index];
    }

    internal void RemoveAllIntantiatedItems()
    {
        foreach (Transform child in transform)
        {
#if UNITY_EDITOR
            if (isActiveAndEnabled)
                StartCoroutine(DestroyGameObjectOnEditor(child.gameObject));
#else
            Destroy(child.gameObject);
#endif
        }
    }

    internal void InstantiateItem(BaseComponentView newItem, string name)
    {
#if UNITY_EDITOR
        if (isActiveAndEnabled)
            StartCoroutine(IntantiateGameObjectOnEditor(newItem, transform, name));
#else
        Instantiate(newItem.gameObject, transform);
#endif
    }

#if UNITY_EDITOR
    private IEnumerator DestroyGameObjectOnEditor(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }

    private IEnumerator IntantiateGameObjectOnEditor(BaseComponentView newItem, Transform parent, string name)
    {
        if (newItem == null)
            yield break;

        yield return null;
        GameObject newGO = Instantiate(newItem.gameObject, parent);
        newGO.name = name;
    }
#endif
}