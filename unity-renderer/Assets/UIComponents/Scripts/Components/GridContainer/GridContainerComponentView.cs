using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public interface IGridContainerComponentView
{
    /// <summary>
    /// Number of items per row that fit with the current grid configuration.
    /// </summary>
    int currentItemsPerRow { get; }

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
    /// Set the item size adaptation to the container.
    /// </summary>
    /// <param name="adaptItemSizeToContainer">True for activate the size adaptation.</param>
    void SetItemSizeToContainerAdaptation(bool adaptItemSizeToContainer);

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
    /// Set the minimum width for the items when constraint is set to flexible.
    /// </summary>
    /// <param name="minWidthForFlexibleItems">Min item width.</param>
    void SetMinWidthForFlexibleItems(float minWidthForFlexibleItems);

    /// <summary>
    /// Set the items of the grid. All previously existing items will be removed.
    /// </summary>
    /// <param name="items">List of UI components.</param>
    void SetItems(List<BaseComponentView> items);

    /// <summary>
    /// Adds a new item in the grid.
    /// </summary>
    /// <param name="item">An UI component.</param>
    void AddItem(BaseComponentView item);

    /// <summary>
    /// Remove an item from the grid.
    /// </summary>
    /// <param name="item">An UI component</param>
    void RemoveItem(BaseComponentView item);

    /// <summary>
    /// Get all the items of the grid.
    /// </summary>
    /// <returns>List of items.</returns>
    List<BaseComponentView> GetItems();

    /// <summary>
    /// Extract all items out of the grid.
    /// </summary>
    /// <returns>The list of extracted items.</returns>
    List<BaseComponentView> ExtractItems();

    /// <summary>
    /// Remove all existing items from the grid.
    /// </summary>
    void RemoveItems();
}

public class GridContainerComponentView : BaseComponentView, IGridContainerComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal GridLayoutGroup gridLayoutGroup;
    [SerializeField] internal RectTransform externalParentToAdaptSize;

    [Header("Configuration")]
    [SerializeField] internal GridContainerComponentModel model;

    internal List<BaseComponentView> instantiatedItems = new List<BaseComponentView>();

    public int currentItemsPerRow { get; internal set; }

    public override void Awake()
    {
        base.Awake();

        RegisterCurrentInstantiatedItems();
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (GridContainerComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetConstraint(model.constraint);
        SetItemSize(model.itemSize);
        SetConstraintCount(model.constraintCount);
        SetSpaceBetweenItems(model.spaceBetweenItems);
    }

    public override void OnScreenSizeChanged()
    {
        base.OnScreenSizeChanged();

        SetItemSize(model.itemSize);
        SetConstraintCount(model.constraintCount);
    }

    public override void Dispose()
    {
        base.Dispose();

        RemoveItems();
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
        model.constraintCount = newConstraintCount;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.constraintCount = newConstraintCount;
    }

    public void SetItemSizeToContainerAdaptation(bool adaptItemSizeToContainer)
    {
        model.adaptItemSizeToContainer = adaptItemSizeToContainer;
        SetItemSize(model.itemSize);
    }

    public void SetItemSize(Vector2 newItemSize)
    {
        Vector2 newSizeToApply = newItemSize;

        if (model.adaptItemSizeToContainer)
        {
            switch (model.constraint)
            {
                case Constraint.FixedColumnCount:
                    CalculateSizeForFixedColumnConstraint(out newSizeToApply);
                    break;
                case Constraint.FixedRowCount:
                    CalculateSizeForFixedRowConstraint(out newSizeToApply);
                    break;
                case Constraint.Flexible:
                    CalculateSizeForFlexibleConstraint(out newSizeToApply, newItemSize);
                    break;
            }
        }

        model.itemSize = newSizeToApply;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.cellSize = newSizeToApply;

        ResizeGridContainer();
    }

    internal void CalculateSizeForFixedColumnConstraint(out Vector2 newSizeToApply)
    {
        float width = externalParentToAdaptSize != null ? externalParentToAdaptSize.rect.width : ((RectTransform)transform).rect.width;
        float extraSpaceToRemove = (model.spaceBetweenItems.x * (model.constraintCount - 1)) / model.constraintCount;

        newSizeToApply = new Vector2(
            (width / model.constraintCount) - extraSpaceToRemove,
            model.itemSize.y);

        currentItemsPerRow = model.constraintCount;
    }

    internal void CalculateSizeForFixedRowConstraint(out Vector2 newSizeToApply)
    {
        float height = ((RectTransform)transform).rect.height;
        float extraSpaceToRemove = (model.spaceBetweenItems.y / (model.constraintCount / 2f));

        newSizeToApply = new Vector2(
            model.itemSize.x,
            (height / model.constraintCount) - extraSpaceToRemove);

        currentItemsPerRow = (int)Mathf.Ceil((float)instantiatedItems.Count / model.constraintCount);
    }

    internal void CalculateSizeForFlexibleConstraint(out Vector2 newSizeToApply, Vector2 newItemSize)
    {
        newSizeToApply = newItemSize;

        float width = externalParentToAdaptSize != null ? externalParentToAdaptSize.rect.width : ((RectTransform)transform).rect.width;
        int numberOfPossibleItems = (int)(width / model.minWidthForFlexibleItems);

        SetConstraint(Constraint.FixedColumnCount);

        if (numberOfPossibleItems > 0)
        {
            for (int numColumnsToTry = 1; numColumnsToTry <= numberOfPossibleItems; numColumnsToTry++)
            {
                SetConstraintCount(numColumnsToTry);
                SetItemSize(model.itemSize);
                currentItemsPerRow = numColumnsToTry;

                if (model.itemSize.x < model.minWidthForFlexibleItems)
                {
                    SetConstraintCount(numColumnsToTry - 1);
                    SetItemSize(model.itemSize);
                    currentItemsPerRow = numColumnsToTry - 1;
                    break;
                }

                newSizeToApply = model.itemSize;
            }
        }
        else
        {
            newSizeToApply = new Vector2(model.minWidthForFlexibleItems, newSizeToApply.y);
        }

        SetConstraint(Constraint.Flexible);
    }

    public void SetSpaceBetweenItems(Vector2 newSpace)
    {
        model.spaceBetweenItems = newSpace;

        if (gridLayoutGroup == null)
            return;

        gridLayoutGroup.spacing = newSpace;
    }

    public void SetMinWidthForFlexibleItems(float minWidthForFlexibleItems)
    {
        model.minWidthForFlexibleItems = minWidthForFlexibleItems;
        SetItemSize(model.itemSize);
    }

    public void SetItems(List<BaseComponentView> items)
    {
        RemoveItems();

        for (int i = 0; i < items.Count; i++)
        {
            CreateItem(items[i], $"Item{i}");
        }

        SetItemSize(model.itemSize);
    }

    public void AddItem(BaseComponentView item)
    {
        CreateItem(item, $"Item{instantiatedItems.Count}");
        SetItemSize(model.itemSize);
    }

    public void RemoveItem(BaseComponentView item)
    {
        BaseComponentView itemToRemove = instantiatedItems.FirstOrDefault(x => x == item);
        if (itemToRemove != null)
        {
            Destroy(itemToRemove.gameObject);
            instantiatedItems.Remove(item);
        }

        SetItemSize(model.itemSize);
    }

    public List<BaseComponentView> GetItems() { return instantiatedItems; }

    public List<BaseComponentView> ExtractItems()
    {
        List<BaseComponentView> extractedItems = new List<BaseComponentView>();
        foreach (BaseComponentView item in instantiatedItems)
        {
            item.transform.SetParent(null);
            extractedItems.Add(item);
        }

        instantiatedItems.Clear();

        return extractedItems;
    }

    public void RemoveItems()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        instantiatedItems.Clear();
    }

    internal void CreateItem(BaseComponentView newItem, string name)
    {
        if (newItem == null)
            return;

        newItem.transform.SetParent(transform);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localScale = Vector3.one;
        newItem.name = name;

        instantiatedItems.Add(newItem);
    }

    internal void ResizeGridContainer()
    {
        int currentNumberOfItems = transform.childCount;

        if (currentNumberOfItems == 0)
        {
            ((RectTransform)transform).sizeDelta = Vector2.zero;
            return;
        }

        if (model.constraint == Constraint.FixedColumnCount)
        {
            int numRows = (int)Mathf.Ceil((float)currentNumberOfItems / model.constraintCount);
            ((RectTransform)transform).sizeDelta = new Vector2(
                model.adaptItemSizeToContainer ? ((RectTransform)transform).sizeDelta.x : (model.constraintCount * model.itemSize.x) + (model.spaceBetweenItems.x * (model.constraintCount - 1)),
                (numRows * model.itemSize.y) + (model.spaceBetweenItems.y * (numRows - 1)));
        }
        else if (model.constraint == Constraint.FixedRowCount)
        {
            int numCols = (int)Mathf.Ceil((float)currentNumberOfItems / model.constraintCount);
            ((RectTransform)transform).sizeDelta = new Vector2(
                (numCols * model.itemSize.x) + (model.spaceBetweenItems.x * (numCols - 1)),
                model.adaptItemSizeToContainer ? ((RectTransform)transform).sizeDelta.y : (model.constraintCount * model.itemSize.y) + (model.spaceBetweenItems.y * (model.constraintCount - 1)));
        }
    }

    internal void RegisterCurrentInstantiatedItems()
    {
        instantiatedItems.Clear();

        foreach (Transform child in transform)
        {
            BaseComponentView existingItem = child.GetComponent<BaseComponentView>();
            if (existingItem != null)
                instantiatedItems.Add(existingItem);
            else
                DestroyImmediate(child.gameObject);
        }

        ResizeGridContainer();
    }
}