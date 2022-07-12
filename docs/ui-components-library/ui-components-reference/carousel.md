# Carousel

Implements: `BaseComponentView, ICarouselComponentView, IComponentModelConfig`

Model: `CarouselComponentModel`

### Preview

![Untitled](carousel/Untitled.png)

### Description

An animated carousel of items ready to automatically (or manually) pass from an item to another one. The contained items can be any other UI Components.

### Configuration (Model)

- `SpaceBetweenItems`: Distance that will be between each item.
- `TimeBetweenItems`: Time (in seconds) that will pass since the carousel shows an item and it pass to the next one.
- `AnimationTransitionTime`: Time (in seconds) the transition animation between items will take.
- `AnimationCurve`: Animation curve used for the transition animation between items.
- `BackgroundColor`: Color of the carousel background.
- `ShowManualControls`: Indicates if the carousel will show the manual controls to switch between items or not.
- `AutomaticTransition`: Indicates if the carousel will automatically switch between items or not.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the carousel component with this data.
- `void SetSpaceBetweenItems(float newSpace)`: Set the distance between carousel items.
- `void SetTimeBetweenItems(float newTime)`: Set the time that will be pass between carousel items.
- `void SetAnimationTransitionTime(float newTime)`: Set the time that will be pass during the transition between items.
- `void SetAnimationCurve(AnimationCurve newCurve)`: Set the animation curve that will be used for the animation between items.
- `void SetBackgroundColor(Color newColor)`: Set the color of the carousel background.
- `void SetManualControlsActive(bool isActived)`: Activates/Deactivates the controls to go to the next/previous item manually.
- `void SetItems(List<BaseComponentView> items)`: Set the items of the carousel.
- `void SetItems(BaseComponentView prefab, int amountOfItems)`: Create the items based on the prefab passed, it will create the amount specified. All previously existing items will be removed.
- `void AddItem(BaseComponentView item)`: Adds a new item in the carousel.
- `void RemoveItem(BaseComponentView item)`: Remove an item from the carousel.
- `List<BaseComponentView> GetItems()`: Get all the items of the carousel.
- `List<BaseComponentView> ExtractItems()`: Extract all items out of the carousel.
- `void RemoveItems()`: Remove all existing items from the carousel.
- `void StartCarousel(int fromIndex, bool startInmediately, CarouselDirection direction, bool changeDirectionAfterFirstTransition, int numberOfInitialJumps)`: Start carousel animation.
- `void StopCarousel()`: Stop carousel animation.
- `void GoToPreviousItem()`: Force the carousel to show the previous item.
- `void GoToNextItem()`: Force the carousel to show the next item.
- `void MakeJumpFromDotsSelector(int numberOfJumps, CarouselDirection direction)`: Force the carousel to jump to a specific item.

### How To Use

1. Drag the prefab to your scene.
2. Drag the desired items as child of the "**Content**" game object in the Carousel hierarchy (these items can be any of our UI Components).
3. Configure the model from the inspector.
4. Click on **[REFRESH]**.