# Button_Common

Implements: `BaseComponentView, IButtonComponentView, IComponentModelConfig`

Model: `ButtonComponentModel`

### Preview

![Untitled](button-common/Untitled.png)

### Description

A normal button used for generic situations.

### Configuration (Model)

- `Text`: Text of the button.
- `Icon`: Sprite that will appear in the right side of the button.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the button component with this data.
- `Button.ButtonClickedEvent onClick`: Event that will be triggered when the button is clicked.
- `void SetText(string newText)`: Set the button text.
- `void SetIcon(Sprite newIcon)`: Set the button icon.
- `void SetInteractable(bool isInteractable)`: Set the button clickable or not.
- `bool IsInteractable()`: Indicates if the button is Interactable or not.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.