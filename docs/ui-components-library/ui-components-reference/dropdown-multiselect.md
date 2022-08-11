# Dropdown_MultiSelect

Implements: `BaseComponentView, IDropdownComponentView, IComponentModelConfig`

Model: `DropdownComponentModel`

### Preview

![Untitled](dropdown-multiselect/Untitled.png)

### Description

A dynamic dropdown list with multi-options to create items selectors.

### Configuration (Model)

- `Title`: Text that will be showed in the dropdown button.
- `SearchPlaceHolderText`: Text that will be showed as place holder in the search bar of the dropdown.
- `searchNotFoundText`: Text that will be showed when a search does not find results.
- `IsMultiselect`: Indicates if the user will be able to select more than one option simultaneously or not.
- `ShowSelectAllOption`: Indicates if the dropdown will activate a first option that select/unselect all the available ones.
- `isOptionsPanelHeightDynamic`: Indicates if the dropdown height will change automatically depending on the amount of items.
- `maxValueForDynamicHeight`: Set a max height of the dropdown when the `isOptionsPanelHeightDynamic` option is activated.
- `emptyContentText`: Text that will be showed when there are no items loaded.
- `Options`: List of options that will be created in the dropdown. Each ption has the following configuration:
    - `Id`: Unique identifier for the option.
    - `Text`: Text that will be showed in the option.
    - `IsOn`: Indicates if the option is selected or not.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the dropdown component with this data.
- `event Action<bool, string> OnOptionSelectionChanged`: Event that will be triggered when the selection of any option changes.
- `bool isMultiselect`: Set the dropdown as multiselect or not.
- `void Open()`: Open the options list.
- `void Close()`: Closes the options list.
- `void SetTitle(string newText)`: Set the dropdown title.
- `void SetOptions(List<ToggleComponentModel> options)`: Set the available options of the dropdown.
- `IToggleComponentView GetOption(int index)`: Get an option of the dropdown.
- `List<IToggleComponentView> GetAllOptions()`: Get all the options of the dropdown.
- `void FilterOptions(string filterText)`: Filter options using a text.
- `void SetSelectAll(bool isSelected)`: Select/unselect all the available options (if multiselect is activated).
- `void SetSearchPlaceHolderText(string newText)`: Set the search bar place holder text.
- `void SetSearchNotFoundText(string newText)`: Set the text for when the search doesn't find anything.
- `void SetEmptyContentText(string newText)`: Set the text for when the dropdown doesn't have items.
- `void SetLoadingActive(bool isActive)`: Show/Hide the loading panel.
- `void SetSelectAllOptionActive(bool isActive)`: Show/Hide the "Select All" option (only for multiselect configuration).
- `void SetOptionsPanelHeightAsDynamic(bool isDynamic, float maxHeight)`: Make the height of the options panel be dynamic depending on the number of instantiated options.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.