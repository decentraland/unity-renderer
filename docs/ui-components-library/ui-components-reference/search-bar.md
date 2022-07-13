# SearchBar

Implements: `BaseComponentView, ISearchBarComponentView, IComponentModelConfig`

Model: `SearchBarComponentModel`

### Preview

![Untitled](search-bar/Untitled.png)

### Description

A search bar used to filter between any kind of data list.

### Configuration (Model)

- `IdleTimeToTriggerSearch`: Time in seconds that the search bar will wait to inform about the text change.
- `PlaceHolderText`: Text that will be showed as place holder in the search bar.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the search bar component with this data.
- `event Action<string> OnSearchText`: Event that will be triggered when a search is ordered in the search component.
- `void SetPlaceHolderText(string value)`: Set the place holder text of the search component.
- `void SubmitSearch(string value)`: Order a specific search.
- `void ClearSearch()`: Clear the search component.
- `void SetIdleSearchTime(float idleSearchTime)`: Set the idle time to search.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.