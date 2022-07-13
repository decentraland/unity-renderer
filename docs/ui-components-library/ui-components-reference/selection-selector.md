# SectionSelector

Implements: `BaseComponentView, ISectionSelectorComponentView, IComponentModelConfig`

Model: `SectionSelectorComponentModel`

### Preview

![Untitled](selection-selector/Untitled.png)

### Description

A dynamic section selector to create menus.

### Configuration (Model)

- `Sections`: List of sections that will be created in the menu. Each section has the following configuration:
    - `Visual Configuration When Selected`
        - `SelectedIcon`: Icon that will be showed in the section toggle when the section is selected.
        - `SelectedTitle`: Text that will be showed in the section toggle when the section is selected.
        - `SelectedTextColor`: Title color when the section is selected.
        - `SelectedImageColor`: Icon color when the section is selected.
        - `BackgroundTransitionColorsForSelected`: Background color when the section is selected.
    - `Visual Configuration When Unselected`
        - `UnselectedIcon`: Icon that will be showed in the section toggle when the section is unselected.
        - `UnselectedTitle`: Text that will be showed in the section toggle when the section is unselected.
        - `UnselectedTextColor`: Title color when the section is unselected.
        - `UnselectedImageColor`: Icon color when the section is unselected.
        - `BackgroundTransitionColorsForUnselected`: Background color when the section is unselected.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the section selector component with this data.
- `void SetSections(List<SectionToggleModel> sections)`: Set the sections of the selector.
- `ISectionToggle GetSection(int index)`: Get a section of the section selector.
- `List<ISectionToggle> GetAllSections()`: Get all the sections of the section selector.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.