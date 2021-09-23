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
    void SetItemSize(Vector2 newItemSize);

    /// <summary>
    /// Set the space between child items.
    /// </summary>
    /// <param name="newSpace">Space between children.</param>
    void SetSpaceBetweenItems(Vector2 newSpace);

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

    /// <summary>
    /// Get all the items of the grid.
    /// </summary>
    /// <returns>The list of items.</returns>
    List<BaseComponentView> GetAllItems();
}

public class GridContainerComponentView : BaseComponentView, IGridContainerComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal GridLayoutGroup gridLayoutGroup;

    [Header("Configuration")]
    [SerializeField] internal GridContainerComponentModel model;

    private List<BaseComponentView> instantiatedItems = new List<BaseComponentView>();

    public override void PostInitialization() { Configure(model); }

    public void Configure(GridContainerComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetNumColumns(model.numColumns);
        SetItemSize(model.itemSize);
        SetSpaceBetweenItems(model.spaceBetweenItems);
        SetItems(model.items);
    }

    public override void Dispose()
    {
        base.Dispose();

        RemoveAllInstantiatedItems();
    }

    public void SetNumColumns(int newNumColumns)
    {
        model.numColumns = newNumColumns;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.constraintCount = newNumColumns;
    }

    public void SetItemSize(Vector2 newItemSize)
    {
        model.itemSize = newItemSize;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.cellSize = newItemSize;
    }

    public void SetSpaceBetweenItems(Vector2 newSpace)
    {
        model.spaceBetweenItems = newSpace;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.spacing = newSpace;
    }

    public void SetItems(List<BaseComponentView> items)
    {
        model.items = items;

        RemoveAllInstantiatedItems();

        for (int i = 0; i < items.Count; i++)
        {
            CreateItem(items[i], $"Item{i}");
        }
    }

    public BaseComponentView GetItem(int index)
    {
        if (index >= instantiatedItems.Count)
            return null;

        return instantiatedItems[index];
    }

    public List<BaseComponentView> GetAllItems() { return instantiatedItems; }

    internal void CreateItem(BaseComponentView newItem, string name)
    {
        if (Application.isPlaying)
        {
            InstantiateItem(newItem, name);
        }
        else
        {
            if (isActiveAndEnabled)
                StartCoroutine(InstantiateItemOnEditor(newItem, name));
        }
    }

    internal void InstantiateItem(BaseComponentView newItem, string name)
    {
        if (newItem == null)
            return;

        BaseComponentView newGO = Instantiate(newItem, transform);
        newGO.name = name;
        instantiatedItems.Add(newGO);
    }

    internal IEnumerator InstantiateItemOnEditor(BaseComponentView newItem, string name)
    {
        yield return null;
        InstantiateItem(newItem, name);
    }

    internal void RemoveAllInstantiatedItems()
    {
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                if (isActiveAndEnabled)
                    StartCoroutine(DestroyGameObjectOnEditor(child.gameObject));
            }
        }

        instantiatedItems.Clear();
    }

    internal IEnumerator DestroyGameObjectOnEditor(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }
}