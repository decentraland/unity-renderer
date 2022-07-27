# Architecture Overview

## In which project folder is everything?

All the code and assets related to the UI Components are included in the `Assets/UIComponents` folder. There we can find the following sub-folders:

- **Resources**: Here we will find the list of prefabs that represent to each UI Component (different kind of buttons, carousel, grid container, image, etc.).
- **Scripts**: All the code related to the logic associated to each UI Component will be contained here.
- **Tests**: All the code related to the tests coverage of each UI Component will be contained here.

> **Important note**: *As well as in `Assets/UIComponents` folder we will have all the **common** UI Components, we can also have **specific** UI Components for a feature that will only be used by this specific feature. In this case we would have its related code and assets inside the folder of the own feature.*
> 

## The choosen Design Pattern

The implemented architecture for UI Components has been designed to be used as part of a **MVC** design pattern, where models (**M**) and views (**V**) would be part of the UI Components side, and controllers (**C**) would be part of the specific implementation that we would need for each feature consumer of UI Components.

## The BaseComponentView base class

All UI Components that we implement inherits from the `BaseComponentView` base class. This class is a `MonoBehaviour` and contains all the common logic that will be shared by all our UIs and it will be the place where we will have to go when we need to add new common behaviours in the future (if needed).

This class implements the following properties/methods through its `IBaseComponentView` interface:

- `isVisible`: It will inform if the UI Component is currently visible or not (the property is readonly).
- `onFocused`: It will be triggered when UI Component is focused.
- `isFocused`: It will inform if the UI Component is focused or not (the property is readonly).
- `Awake()`: It is called at the beginning of the UI Component lifecycle.
- `Update()`: It is called once per frame.
- `OnEnable()`: It is called each time the UI Component is enabled.
- `OnDisable()`: It is called each time the UI Component is disabled.
- `Start()`: It is called just after the UI Component has been initialized.
- `RefreshControl()`: This method is `abstract` and is implemented in each specific UI Component to update all the visuals parts of the component with its current model configuration. This method is called by the [REFRESH] button that all the UI Components have in its own inspector (it will be explained below).
- `Show(bool instant)`: Shows the UI component. It uses the `ShowHideAnimator` component by default (if we add it as part of the UI Component prefab), but it can be overridden to implement other custom ways of showing the UI.
- `Hide(bool instant)`: Hides the UI component. It uses the `ShowHideAnimator` component by default (if we add it as part of the UI Component prefab), but it can be overridden to implement other custom ways of hiding the UI.
- `OnFocus()`: It is called when the focus is set into the component (OnPointerEnter).
- `OnLoseFocus()`: It is called when the focus is lost from the component (OnPointerExit).
- `OnScreenSizeChanged()`: It is called just after the screen size has changed.

## UI Components inspector

All UI Components, due to inherits from the `BaseComponentView`, have a **[REFRESH]** button exposed in its own inspector. This button is calling to the `RefreshControl()` implemented for the specific UI Component and is useful for designers to be able to update the component in edition mode.

> **Example**: A designer could drag the UI Component "Button_Common" and drop it into his scene. After that he would set the model that this UI Component is exposing through its inspector with the specific values that he want (in this case the text and icon for the button). Once the model is configured, he would click on the [REFRESH] button and automatically the button would be refreshed with the data.
> 

![Untitled](architecture-overview/Untitled.png)

![Untitled](architecture-overview/Untitled%201.png)

![Untitled](architecture-overview/Untitled%202.png)