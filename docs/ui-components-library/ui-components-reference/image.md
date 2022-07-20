# Image

Implements: `BaseComponentView, IImageComponentView, IComponentModelConfig`

Model: `ImageComponentModel`

### Preview

![Untitled](image/Untitled.png)

### Description

A normal image used for generic situations. It is ready to be loaded from different sources as `Sprite`, `Texture2D` or `Url`. It also includes a loading indicator.

### Configuration (Model)

- `Sprite`: Image source (for images from a `Sprite`).
- `Texture`: Image source (for images from a `Texture`).
    - **NOTE**: It only applies if `Sprite` is set as `null`.
- `Uri`: Image source (for images from an `url`).
    - **NOTE**: It only applies if `Sprite` and `Texture` are set as `null`.
- `LastUriCached`: Indicates if we want to cache the last uri request.
- `FitParent`: Indicates if we want to make the image automatically fills the width and height of the parent container.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the image component with this data.
- `event Action<Sprite> OnLoaded`: It will be triggered when the sprite has been loaded.
- `void SetImage(Sprite sprite)`: Set an image directly from a sprite.
- `void SetImage(Texture2D texture)`: Set an image from a 2D texture.
- `void SetImage(string uri)`: Set an image from an uri.
- `void SetLastUriRequestCached(bool isEnabled)`: Set if we want to enable the caching of the last uri request.
- `void SetFitParent(bool fitParent)`: Resize the image size to fit into the parent.
- `void SetLoadingIndicatorVisible(bool isVisible)`: Active or deactive the loading indicator.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.