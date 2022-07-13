# ProfileCard

Implements: `BaseComponentView, IProfileCardComponentView, IComponentModelConfig`

Model: `ProfileCardComponentModel`

### Preview

![Untitled](profile-card/Untitled.png)

### Description

A card that represents an user profile.

### Configuration (Model)

- `ProfilePictureSprite`: Profile image source (for images from a `Sprite`).
- `ProfilePictureTexture`: Profile image source (for images from a `Texture`).
    - **NOTE**: It only applies if `ProfilePictureSprite` is set as `null`.
- `ProfilePictureUri`: Image source (for images from an `url`).
    - **NOTE**: It only applies if `ProfilePictureSprite` and `ProfilePictureTexture` are set as `null`.
- `ProfileName`: Name of the user.
- `ProfileAddress`: Wallet address of the user.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the profile card component with this data.
- `Button.ButtonClickedEvent onClick`: Event that will be triggered when the profile card is clicked.
- `void SetProfilePicture(Sprite sprite)`: Set the profile picture directly from a sprite.
- `void SetProfilePicture(Texture2D newPicture)`: Set the profile picture from a 2D texture.
- `void SetProfilePicture(string uri)`: Set the profile picture from an uri.
- `void SetProfileName(string newName)`: Set the profile name.
- `void SetProfileAddress(string newAddress)`: Set the profile address. It will only show the last 4 caracteres.
- `void SetLoadingIndicatorVisible(bool isVisible)`: Active or deactive the loading indicator.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.