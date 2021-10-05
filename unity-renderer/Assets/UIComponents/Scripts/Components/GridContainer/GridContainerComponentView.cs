using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public interface IGridContainerComponentView
{
    /// <summary>
    /// Fill the model and updates the grid with this data.
    /// </summary>
    /// <param name="model">Data to configure the grid.</param>
    void Configure(GridContainerComponentModel model);

    /// <summary>
    /// Set the type of constraint of the grid.
    /// </summary>
    /// <param name="newConstraint">Type of constraint.</param>
    void SetConstraint(Constraint newConstraint);

    /// <summary>
    /// Set the number of columns/rows of the grid depending on the type of constraint set.
    /// </summary>
    /// <param name="newConstraintCount">Number of columns or rows.</param>
    void SetConstraintCount(int newConstraintCount);

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
    /// <param name="instantiateNewCopyOfItems">Indicates if the items provided will be instantiated as a new copy or not.</param>
    void SetItems(List<BaseComponentView> items, bool instantiateNewCopyOfItems = true);

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
    [SerializeField] internal RectTransform externalScrollViewContainer;

    [Header("Configuration")]
    [SerializeField] internal GridContainerComponentModel model;

    internal List<BaseComponentView> instantiatedItems = new List<BaseComponentView>();
    internal bool destroyOnlyUnnecesaryItems = false;

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

        SetConstraint(model.constraint);
        SetItems(model.items);
        SetConstraintCount(model.constranitCount);
        SetSpaceBetweenItems(model.spaceBetweenItems);
    }

    public override void Dispose()
    {
        base.Dispose();

        DestroyInstantiatedItems(true);
    }

    public void SetConstraint(Constraint newConstraint)
    {
        model.constraint = newConstraint;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.constraint = newConstraint;
    }

    public void SetConstraintCount(int newConstraintCount)
    {
        model.constranitCount = newConstraintCount;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.constraintCount = newConstraintCount;
    }

    public void SetItemSize(Vector2 newItemSize)
    {
        Vector2 newSizeToApply = newItemSize;
        if (model.adaptItemSizeToGridSize)
        {
            if (model.constraint == Constraint.FixedColumnCount)
            {
                float width = externalScrollViewContainer != null ? externalScrollViewContainer.rect.width : ((RectTransform)transform).rect.width;
                float extraSpaceToRemove = (model.spaceBetweenItems.x / (model.constranitCount / 2f));
                newSizeToApply = new Vector2(
                    (width / model.constranitCount) - extraSpaceToRemove,
                    model.itemSize.y);
            }
            else if (model.constraint == Constraint.FixedRowCount)
            {
                float height = ((RectTransform)transform).rect.height;
                float extraSpaceToRemove = (model.spaceBetweenItems.y / (model.constranitCount / 2f));
                newSizeToApply = new Vector2(
                    model.itemSize.x,
                    (height / model.constranitCount) - extraSpaceToRemove);
            }
        }

        model.itemSize = newSizeToApply;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.cellSize = newSizeToApply;

        ResizeGridContainer();
    }

    public void SetSpaceBetweenItems(Vector2 newSpace)
    {
        model.spaceBetweenItems = newSpace;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.spacing = newSpace;
    }

    public void SetItems(List<BaseComponentView> items, bool instantiateNewCopyOfItems = true)
    {
        model.items = items;

        DestroyInstantiatedItems(!destroyOnlyUnnecesaryItems);

        for (int i = 0; i < items.Count; i++)
        {
            CreateItem(items[i], $"Item{i}", instantiateNewCopyOfItems && !destroyOnlyUnnecesaryItems);
        }

        ResizeGridContainer();

        if (!instantiateNewCopyOfItems)
            destroyOnlyUnnecesaryItems = true;

        SetItemSize(model.itemSize);
    }

    public BaseComponentView GetItem(int index)
    {
        if (index >= instantiatedItems.Count)
            return null;

        return instantiatedItems[index];
    }

    public List<BaseComponentView> GetAllItems() { return instantiatedItems; }

    internal void CreateItem(BaseComponentView newItem, string name, bool instantiateNewCopyOfItem = true)
    {
        if (Application.isPlaying)
        {
            InstantiateItem(newItem, name, instantiateNewCopyOfItem);
        }
        else
        {
            if (isActiveAndEnabled)
                StartCoroutine(InstantiateItemOnEditor(newItem, name));
        }
    }

    internal void InstantiateItem(BaseComponentView newItem, string name, bool instantiateNewCopyOfItem = true)
    {
        if (newItem == null)
            return;

        BaseComponentView newGO;
        if (instantiateNewCopyOfItem)
        {
            newGO = Instantiate(newItem, transform);
        }
        else
        {
            newGO = newItem;
            newGO.transform.SetParent(transform);
            newGO.transform.localPosition = Vector3.zero;
            newGO.transform.localScale = Vector3.one;
        }

        newGO.name = name;
        instantiatedItems.Add(newGO);
    }

    internal IEnumerator InstantiateItemOnEditor(BaseComponentView newItem, string name)
    {
        yield return null;
        InstantiateItem(newItem, name);
    }

    internal void DestroyInstantiatedItems(bool forzeToDestroyAll)
    {
        if (forzeToDestroyAll)
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
        }
        else
        {
            foreach (BaseComponentView child in instantiatedItems)
            {
                if (!model.items.Contains(child))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        instantiatedItems.Clear();
    }

    internal IEnumerator DestroyGameObjectOnEditor(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }

    internal void ResizeGridContainer()
    {
        if (model.items.Count == 0)
        {
            ((RectTransform)transform).sizeDelta = Vector2.zero;
            return;
        }

        if (model.constraint == Constraint.FixedColumnCount)
        {
            int numRows = (int)Mathf.Ceil((float)model.items.Count / model.constranitCount);
            ((RectTransform)transform).sizeDelta = new Vector2(
                model.adaptItemSizeToGridSize ? ((RectTransform)transform).sizeDelta.x : (model.constranitCount * model.itemSize.x) + (model.spaceBetweenItems.x * (model.constranitCount - 1)),
                (numRows * model.itemSize.y) + (model.spaceBetweenItems.y * (numRows - 1)));
        }
        else if (model.constraint == Constraint.FixedRowCount)
        {
            int numCols = (int)Mathf.Ceil((float)model.items.Count / model.constranitCount);
            ((RectTransform)transform).sizeDelta = new Vector2(
                (numCols * model.itemSize.x) + (model.spaceBetweenItems.x * (numCols - 1)),
                model.adaptItemSizeToGridSize ? ((RectTransform)transform).sizeDelta.y : (model.constranitCount * model.itemSize.y) + (model.spaceBetweenItems.y * (model.constranitCount - 1)));
        }
    }
}