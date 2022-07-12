# Tag

Implements: `BaseComponentView, ITagComponentView, IComponentModelConfig`

Model: `TagComponentModel`

### Preview

![Untitled](tag/Untitled.png)

### Description

An info tag.

### Configuration (Model)

- `Text`: Text of the tag.
- `Icon`: Sprite that will appear in the left side of the tag.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the tag component with this data.
- `void SetText(string newText)`: Set the tag text.
- `void SetIcon(Sprite newIcon)`: Set the tag icon.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.