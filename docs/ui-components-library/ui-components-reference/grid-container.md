# GridContainer

Implements: `BaseComponentView, IGridContainerComponentView, IComponentModelConfig`

Model: `GridContainerComponentModel`

### Preview

![Untitled](grid-container/Untitled.png)

### Description

A grid container of items ready to automatically organize them with different configurations. The contained items can be any other UI Components.

### Configuration (Model)

- `Constraint`: Sets the way the items will be positioned in the grid.
    - `Flexible`: The items will be positioned depending on the grid width.
    - `FixedColumnCount`: The items will be positioned respecting the configured number of columns.
    - `FixedRowCount`: The items will be positioned respecting the configured number of rows.
- `ConstraintCount`: Represents the number of fixed columns/rows that we want to configure.
    - **NOTE**: It only applies if `Constraint` is set as `FixedColumnCount` or `FixedRowCount`
- `AdaptItemSizeToContainer`: If it is set to `true`, the width of the items will try to be adapted to the width of the grid.
    - **NOTE**: It is is set to `true`, the `x` value of the `ItemSize` parameter will be ignored.
- `ItemSize`: Configures the size of the items included in the grid.
    - `X`: Items width. It only applies if `AdaptItemSizeToContainer` is set to false.
    - `Y`: Items height.
- `SpaceBetweenItems`: Configures the distance between items.
    - `X`: Horizontal distance.
    - `Y`: Vertical distance.
- `MinWidthForFlexibleItems`: It will be the minimum width that the items will try to have when the grid is configured as `Flexible`.
    - **NOTE**: It only applies if `Constraint` is set to `Flexible` and `AdaptItemSizeToContainer` is also set to `true`. This kind of configuration is commonly used to configure the grid as a "responsive" UI.
- `ExternalParentToAdaptSize`: If it is set with something, the `AdaptItemSizeToContainer` configuration will take into account the width of the set object instead of the grid width.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the grid component with this data.
- `int currentItemsPerRow`: Number of items per row that fit with the current grid configuration.
- `void SetConstraint(Constraint newConstraint)`: Set the type of constraint of the grid.
- `void SetConstraintCount(int newConstraintCount)`: Set the number of columns/rows of the grid depending on the type of constraint set.
- `void SetItemSizeToContainerAdaptation(bool adaptItemSizeToContainer)`: Set the item size adaptation to the container.
- `void SetItemSize(Vector2 newItemSize)`: Set the size of each child item.
- `void SetSpaceBetweenItems(Vector2 newSpace)`: Set the minimum width for the items when constraint is set to flexible.
- `void SetItems(BaseComponentView prefab, int amountOfItems)`: Create the items based on the prefab passed, it will create the amount specified. All previously existing items will be removed.
- `void SetItems(List<BaseComponentView> items)`: Set the items of the grid. All previously existing items will be removed.
- `void AddItem(BaseComponentView item)`: Adds a new item in the grid.
- `void RemoveItem(BaseComponentView item)`: Remove an item from the grid.
- `List<BaseComponentView> GetItems()`: Get all the items of the grid.
- `List<BaseComponentView> ExtractItems()`: Extract all items out of the grid.
- `void RemoveItems()`: Remove all existing items from the grid.

### How To Use

1. Drag the prefab to your scene.
2. Drag the desired items as child of the GridContainer game object  (these items can be any of our UI Components).
3. Configure the model from the inspector.
4. Click on **[REFRESH]**.