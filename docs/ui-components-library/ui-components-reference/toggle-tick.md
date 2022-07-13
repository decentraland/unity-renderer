# Toggle_Tick

Implements: `BaseComponentView, IToggleComponentView, IComponentModelConfig`

Model: `ToggleComponentModel`

### Preview

![Untitled](toggle-tick/Untitled.png)

### Description

A tick toggle used for items selections.

### Configuration (Model)

- `Id`: Unique identifier for the checkbox.
- `Text`: Text that will be showed in the checkbox.
- `IsOn`: Indicates if the checkbox is selected or not.
- `isTextActive`: Boolean that sets the text active or not.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the toggle component with this data.
- `event Action<bool, string> OnSelectedChanged`: Event that will be triggered when the toggle changes.
- `string Id`: Id asociated to the toogle.
- `bool IsOn`: On/Off state of the toggle.
- `string Title`: Text (if exists) associated to the toggle.
- `void SetText(string newText)`: Set the toggle text.
- `void SetTextActive(bool isActive)`: Set toggle text active.
- `void SetInteractable(bool isInteractable)`: Set the toggle clickable or not.
- `bool IsInteractable()`: Return if the toggle is Interactable or not.
- `void SetIsOnWithoutNotify(bool isOn)`: Set the state of the toggle as On/Off without notify event.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.