# ColumnsOrganizer

Implements: `BaseComponentView, IColumnsOrganizerComponentView`

Model: `ColumnsOrganizerComponentModel`

### Preview

![Untitled](columns-organizer/Untitled.png)

### Description

A columns organizer that allow us to set several columns with different widths and based on fixed or percentages values. The widths calculation will be re-evaluated each time tje screen size changes.

### Configuration (Model)

- `spaceBetweenColumns`: Configures the distance between columns.
- `columnsConfig`: List of configurations that will be applied to the columns. Each colums config has the following parameters:
    - `isPercentage`: Indicates if the column width will be calculated as a percentage of the total width of the columns of type "percentage", or simply as a fixed value.
    - `width`: Width that will be applied to the column.
- `uiComponentsToRefreshSize` (optional): List of components to which we want to refresh their size each time the `RecalculateColumnsSize()` method is called.

### Exposed Properties/Methods

- `void RecalculateColumnsSize()`: Recalculates the size of each columns following the configuration in the model.

### How To Use

1. Drag the prefab to your scene.
2. Create the desired columns as child of the ColumnsOrganizer game object  (these columns can be any Unity game object).
3. Configure the model from the inspector.
4. Click on **[REFRESH]**.