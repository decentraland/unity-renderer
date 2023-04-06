using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public interface ICarouselComponentView
{
    /// <summary>
    /// Set the distance between carousel items.
    /// </summary>
    /// <param name="newSpace">Distance between items.</param>
    void SetSpaceBetweenItems(float newSpace);

    /// <summary>
    /// Set the time that will be pass between carousel items.
    /// </summary>
    /// <param name="newTime">Time between items.</param>
    void SetTimeBetweenItems(float newTime);

    /// <summary>
    /// Set the time that will be pass during the transition between items.
    /// </summary>
    /// <param name="newTime">Transition time between items.</param>
    void SetAnimationTransitionTime(float newTime);

    /// <summary>
    /// Set the animation curve that will be used for the animation between items.
    /// </summary>
    /// <param name="newCurve">Animation curve between items.</param>
    void SetAnimationCurve(AnimationCurve newCurve);

    /// <summary>
    /// Set the color of the carousel background.
    /// </summary>
    /// <param name="newColor">Background color.</param>
    void SetBackgroundColor(Color newColor);

    /// <summary>
    /// Activates/Deactivates the controls to go to the next/previous item manually.
    /// </summary>
    /// <param name="isActived">True for activating the manual controls.</param>
    void SetManualControlsActive(bool isActived);

    /// <summary>
    /// Set the items of the carousel.
    /// </summary>
    /// <param name="items">List of UI components.</param>
    void SetItems(List<BaseComponentView> items);

    /// <summary>
    /// Creates the items of the carousel from the prefab. All previously existing items will be removed.
    /// </summary>
    /// <param name="prefab">Prefab to create items</param>
    /// <param name="amountOfItems">Amounts of items to be created</param>
    void SetItems(BaseComponentView prefab, int amountOfItems);

    /// <summary>
    /// Adds a new item in the carousel.
    /// </summary>
    /// <param name="item">An UI component.</param>
    void AddItem(BaseComponentView item);

    /// <summary>
    /// Adds a new item in the carousel and update carousel dot selector.
    /// </summary>
    /// <param name="item">An UI component.</param>
    void AddItemWithDotsSelector(BaseComponentView item);

    /// <summary>
    /// Remove an item from the carousel.
    /// </summary>
    /// <param name="item">An UI component</param>
    void RemoveItem(BaseComponentView item);

    /// <summary>
    /// Get all the items of the carousel.
    /// </summary>
    /// <returns>The list of items.</returns>
    List<BaseComponentView> GetItems();

    /// <summary>
    /// Extract all items out of the carousel.
    /// </summary>
    /// <returns>The list of extracted items.</returns>
    List<BaseComponentView> ExtractItems();

    /// <summary>
    /// Remove all existing items from the carousel.
    /// </summary>
    void RemoveItems();

    /// <summary>
    /// Start carousel animation.
    /// </summary>
    /// <param name="fromIndex">It specifies from where item the carousel will start.</param>
    /// <param name="startInmediately">True to directly execute the first transition.</param>
    /// <param name="direction">Set the direction of the carousel animations: right or left.</param>
    /// <param name="changeDirectionAfterFirstTransition">True to change the carousel direction just after the first transition.</param>
    /// <param name="numberOfInitialJumps">Number of jumps that will be executed in the first transition.</param>
    void StartCarousel(int fromIndex, bool startInmediately, CarouselDirection direction, bool changeDirectionAfterFirstTransition, int numberOfInitialJumps);

    /// <summary>
    /// Stop carousel animation.
    /// </summary>
    void StopCarousel();

    /// <summary>
    /// Force the carousel to show the previous item.
    /// </summary>
    void GoToPreviousItem();

    /// <summary>
    /// Force the carousel to show the next item.
    /// </summary>
    void GoToNextItem();

    /// <summary>
    /// Force the carousel to jump to a specific item.
    /// </summary>
    /// <param name="numberOfJumps">Number of jumps that will be executed during the transition.</param>
    /// <param name="direction">Direction in which to make the jumps.</param>
    void MakeJumpFromDotsSelector(int numberOfJumps, CarouselDirection direction);
}

public enum CarouselDirection
{
    Right,
    Left
}

public class CarouselComponentView : BaseComponentView, ICarouselComponentView, IComponentModelConfig<CarouselComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal RectTransform itemsContainer;
    [SerializeField] internal HorizontalLayoutGroup horizontalLayout;
    [SerializeField] internal ScrollRect itemsScroll;
    [SerializeField] internal RectTransform viewport;
    [SerializeField] internal Image background;
    [SerializeField] internal Button previousButton;
    [SerializeField] internal Button nextButton;
    [SerializeField] internal HorizontalLayoutGroup dotsSelector;
    [SerializeField] internal Button dotButtonTemplate;
    [SerializeField] internal Color dotSelectedColor;
    [SerializeField] internal Color dotUnselectedColor;
    [SerializeField] internal bool showOnFocus = false;

    [Header("Configuration")]
    [SerializeField] internal CarouselComponentModel model;

    internal List<BaseComponentView> instantiatedItems = new List<BaseComponentView>();
    internal Coroutine itemsCoroutine;
    internal int currentItemIndex = 0;
    internal int currentDotIndex = 0;
    internal float currentFinalNormalizedPos;
    internal bool isInTransition = false;

    public override void Awake()
    {
        base.Awake();

        StartCoroutine(RegisterCurrentInstantiatedItems());
        ConfigureManualButtonsEvents();
    }

    public void Start()
    {
        if (model.automaticTransition)
            StartCarousel();
    }

    public void Configure(CarouselComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetSpaceBetweenItems(model.spaceBetweenItems);
        SetTimeBetweenItems(model.timeBetweenItems);
        SetAnimationTransitionTime(model.animationTransitionTime);
        SetAnimationCurve(model.animationCurve);
        SetBackgroundColor(model.backgroundColor);
        SetManualControlsActive(model.showManualControls);
        ResizeAllItems();
        GenerateDotsSelector();
    }

    public override void OnScreenSizeChanged()
    {
        base.OnScreenSizeChanged();

        ResizeAllItems();
    }

    public override void Dispose()
    {
        base.Dispose();

        StopCarousel();
        DestroyInstantiatedItems();
    }

    public void SetSpaceBetweenItems(float newSpace)
    {
        model.spaceBetweenItems = newSpace;

        if (horizontalLayout == null)
            return;

        horizontalLayout.spacing = newSpace;
    }

    public void SetTimeBetweenItems(float newTime)
    {
        model.timeBetweenItems = newTime;
    }

    public void SetAnimationTransitionTime(float newTime)
    {
        model.animationTransitionTime = newTime;
    }

    public void SetAnimationCurve(AnimationCurve newCurve)
    {
        model.animationCurve = newCurve;
    }

    public void SetBackgroundColor(Color newColor)
    {
        model.backgroundColor = newColor;

        if (background == null)
            return;

        background.color = newColor;
    }

    public void SetManualControlsActive() =>
        SetManualControlsActive(model.showManualControls);

    public void SetManualControlsActive(bool isActived)
    {
        model.showManualControls = isActived;

        if (previousButton == null || nextButton == null)
            return;

        int currentNumberOfItems = itemsContainer.childCount;
        previousButton.gameObject.SetActive(isActived && currentNumberOfItems > 1);
        nextButton.gameObject.SetActive(isActived && currentNumberOfItems > 1);
        dotsSelector.gameObject.SetActive(isActived && currentNumberOfItems > 1);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (previousButton == null || nextButton == null || !showOnFocus)
            return;

        int currentNumberOfItems = itemsContainer.childCount;
        previousButton.gameObject.SetActive(currentNumberOfItems > 1);
        nextButton.gameObject.SetActive(currentNumberOfItems > 1);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (previousButton == null || nextButton == null || !showOnFocus)
            return;

        previousButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
    }

    public void SetItems(BaseComponentView prefab, int amountOfItems)
    {
        DestroyInstantiatedItems();

        for (int i = 0; i < amountOfItems; i++)
        {
            BaseComponentView instanciatedItem = Instantiate(prefab);
            CreateItem(instanciatedItem, $"Item{i}");
        }

        SetManualControlsActive(model.showManualControls);
        GenerateDotsSelector();
    }

    public void SetItems(List<BaseComponentView> items)
    {
        DestroyInstantiatedItems();

        for (int i = 0; i < items.Count; i++)
        {
            CreateItem(items[i], $"Item{i}");
        }

        SetManualControlsActive(model.showManualControls);
        GenerateDotsSelector();
    }

    public void AddItemWithDotsSelector(BaseComponentView item)
    {
        CreateItem(item, $"Item{instantiatedItems.Count}");
        SetManualControlsActive(model.showManualControls);
        GenerateDotsSelector();
    }

    public void AddItem(BaseComponentView item) =>
        CreateItem(item, $"Item{instantiatedItems.Count}");

    public void RemoveItem(BaseComponentView item)
    {
        BaseComponentView itemToRemove = instantiatedItems.FirstOrDefault(x => x == item);
        if (itemToRemove != null)
        {
            Destroy(itemToRemove.gameObject);
            instantiatedItems.Remove(item);
        }

        SetManualControlsActive(model.showManualControls);
        GenerateDotsSelector();
    }

    public List<BaseComponentView> GetItems() { return instantiatedItems; }

    public List<BaseComponentView> ExtractItems()
    {
        List<BaseComponentView> extractedItems = new List<BaseComponentView>();
        foreach (BaseComponentView item in instantiatedItems)
        {
            if (item != null)
                item.transform.SetParent(null);

            extractedItems.Add(item);
        }

        instantiatedItems.Clear();
        SetManualControlsActive(model.showManualControls);

        return extractedItems;
    }

    public void RemoveItems()
    {
        DestroyInstantiatedItems();
        SetManualControlsActive(model.showManualControls);
    }

    public void StartCarousel(
        int fromIndex = 0,
        bool startInmediately = false,
        CarouselDirection direction = CarouselDirection.Right,
        bool changeDirectionAfterFirstTransition = false,
        int numberOfInitialJumps = 1)
    {
        StopCarousel();

        if (isActiveAndEnabled)
            itemsCoroutine = StartCoroutine(RunCarouselCoroutine(fromIndex, startInmediately, direction, changeDirectionAfterFirstTransition, numberOfInitialJumps));
    }

    public void StopCarousel()
    {
        if (itemsCoroutine == null)
            return;

        StopCoroutine(itemsCoroutine);

        itemsCoroutine = null;
        isInTransition = false;
    }

    public void GoToPreviousItem()
    {
        if (isInTransition)
            return;

        StartCarousel(
            fromIndex: currentItemIndex,
            startInmediately: true,
            direction: CarouselDirection.Left,
            changeDirectionAfterFirstTransition: true,
            numberOfInitialJumps: 1);
    }

    public void ResetCarousel()
    {
        int index = 0;
        SetSelectedDot(index);
    }

    public void GoToNextItem()
    {
        if (isInTransition)
            return;

        StartCarousel(
            fromIndex: currentItemIndex,
            startInmediately: true,
            direction: CarouselDirection.Right,
            changeDirectionAfterFirstTransition: false,
            numberOfInitialJumps: 1);
    }

    public void MakeJumpFromDotsSelector(int numberOfJumps, CarouselDirection direction)
    {
        if (isInTransition)
            return;

        StartCarousel(
            fromIndex: currentItemIndex,
            startInmediately: true,
            direction: direction,
            changeDirectionAfterFirstTransition: direction == CarouselDirection.Left,
            numberOfInitialJumps: numberOfJumps);
    }

    internal void ConfigureManualButtonsEvents()
    {
        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(GoToPreviousItem);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(GoToNextItem);
        }
    }

    internal void CreateItem(BaseComponentView newItem, string name)
    {
        if (newItem == null)
            return;

        newItem.transform.SetParent(itemsContainer);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localScale = Vector3.one;
        newItem.name = name;

        instantiatedItems.Add(newItem);

        ResizeItem((RectTransform)newItem.transform);
    }

    internal void ResizeItem(RectTransform item)
    {
        ((RectTransform)item.transform).sizeDelta = new Vector2(viewport.rect.width, viewport.rect.height);

        int currentNumberOfItems = itemsContainer.childCount;
        itemsContainer.offsetMin = Vector2.zero;
        float extraSpace = (currentNumberOfItems - 1) * model.spaceBetweenItems;
        itemsContainer.offsetMax = new Vector2(viewport.rect.width * (currentNumberOfItems - 1) + extraSpace, 0);
    }

    internal void ResizeAllItems()
    {
        if (itemsScroll.horizontalNormalizedPosition != 0f)
            itemsScroll.horizontalNormalizedPosition = 0f;

        if (model.automaticTransition)
            StartCarousel();

        foreach (Transform child in itemsContainer)
        {
            ResizeItem((RectTransform)child);
        }
    }

    internal void DestroyInstantiatedItems()
    {
        List<BaseComponentView> itemsToDestroy = ExtractItems();

        foreach (BaseComponentView itemToDestroy in itemsToDestroy)
        {
            if (itemToDestroy != null)
                DestroyImmediate(itemToDestroy.gameObject);
        }

        itemsToDestroy.Clear();

        instantiatedItems.Clear();

        itemsContainer.offsetMin = Vector2.zero;
        itemsContainer.offsetMax = Vector2.zero;
    }

    internal IEnumerator RunCarouselCoroutine(
        int fromIndex = 0,
        bool startInmediately = false,
        CarouselDirection direction = CarouselDirection.Right,
        bool changeDirectionAfterFirstTransition = false,
        int numberOfInitialJumps = 1)
    {
        currentItemIndex = fromIndex;
        SetSelectedDot(currentItemIndex);
        bool continueCarrousel = true;
        while (gameObject.activeInHierarchy && itemsContainer.childCount > 1 && continueCarrousel)
        {
            float elapsedTime = 0f;

            if (!startInmediately)
            {
                while (elapsedTime < model.timeBetweenItems)
                {
                    if (!model.pauseOnFocus || (model.pauseOnFocus && !isFocused))
                        elapsedTime += Time.deltaTime;

                    yield return null;
                }

            }

            if (instantiatedItems.Count > 0)
            {
                if (direction == CarouselDirection.Right)
                {
                    SetSelectedDot(currentItemIndex == (instantiatedItems.Count - 1) ? 0 : currentItemIndex + numberOfInitialJumps);
                    yield return RunRightAnimation(numberOfInitialJumps);

                    if (changeDirectionAfterFirstTransition)
                    {
                        direction = CarouselDirection.Left;
                        changeDirectionAfterFirstTransition = false;
                    }
                    continueCarrousel = model.automaticTransition;
                }
                else
                {
                    SetSelectedDot(currentItemIndex == 0 ? (instantiatedItems.Count - 1) : currentItemIndex - numberOfInitialJumps);
                    yield return RunLeftAnimation(numberOfInitialJumps);

                    if (changeDirectionAfterFirstTransition)
                    {
                        direction = CarouselDirection.Right;
                        changeDirectionAfterFirstTransition = false;
                    }
                    continueCarrousel = model.automaticTransition;
                }
            }

            startInmediately = false;
            numberOfInitialJumps = 1;
        }
    }

    internal IEnumerator RunRightAnimation(int numberOfJumps = 1)
    {
        if (currentItemIndex == instantiatedItems.Count - 1)
        {
            currentItemIndex = 0;
            yield return RunAnimationCoroutine(CarouselDirection.Left, instantiatedItems.Count - 1);
        }
        else
        {
            currentItemIndex += numberOfJumps;
            yield return RunAnimationCoroutine(CarouselDirection.Right, numberOfJumps);
        }
    }

    internal IEnumerator RunLeftAnimation(int numberOfJumps = 1)
    {
        if (currentItemIndex == 0)
        {
            currentItemIndex = instantiatedItems.Count - 1;
            yield return RunAnimationCoroutine(CarouselDirection.Right, instantiatedItems.Count - 1);
        }
        else
        {
            currentItemIndex -= numberOfJumps;
            yield return RunAnimationCoroutine(CarouselDirection.Left, numberOfJumps);
        }
    }

    internal IEnumerator RunAnimationCoroutine(CarouselDirection direction, int numberOfJumps = 1)
    {
        isInTransition = true;
        float currentAnimationTime = 0f;
        float initialNormalizedPos = itemsScroll.horizontalNormalizedPosition;

        if (direction == CarouselDirection.Right)
            currentFinalNormalizedPos = initialNormalizedPos + ((float)numberOfJumps / (instantiatedItems.Count - 1));
        else
            currentFinalNormalizedPos = initialNormalizedPos - ((float)numberOfJumps / (instantiatedItems.Count - 1));

        while (currentAnimationTime <= model.animationTransitionTime)
        {
            itemsScroll.horizontalNormalizedPosition = Mathf.Clamp01(Mathf.Lerp(
                initialNormalizedPos,
                currentFinalNormalizedPos,
                model.animationCurve.Evaluate(currentAnimationTime / model.animationTransitionTime)));

            currentAnimationTime += Time.deltaTime;

            yield return null;
        }

        itemsScroll.horizontalNormalizedPosition = currentFinalNormalizedPos;
        isInTransition = false;
    }

    public void GenerateDotsSelector()
    {
        List<GameObject> dotsToRemove = new List<GameObject>();
        foreach (Transform child in dotsSelector.transform)
        {
            if (child.gameObject == dotButtonTemplate.gameObject)
                continue;

            dotsToRemove.Add(child.gameObject);
        }

        foreach (GameObject dotToRemove in dotsToRemove)
        {
            Utils.SafeDestroy(dotToRemove);
        }

        for (int i = 0; i < itemsContainer.childCount; i++)
        {
            Button newDotButton = Instantiate(dotButtonTemplate, dotsSelector.transform);
            newDotButton.gameObject.SetActive(true);
            newDotButton.onClick.AddListener(() =>
            {
                int dotButtonIndex = newDotButton.transform.GetSiblingIndex() - 1;
                if (dotButtonIndex != currentDotIndex)
                {
                    MakeJumpFromDotsSelector(
                        Mathf.Abs(dotButtonIndex - currentDotIndex),
                        dotButtonIndex > currentDotIndex ? CarouselDirection.Right : CarouselDirection.Left);
                }
            });
        }

        SetSelectedDot(0);
    }

    internal void SetSelectedDot(int index)
    {
        int currentIndex = 0;
        currentDotIndex = -1;
        foreach (Transform child in dotsSelector.transform)
        {
            if (child.gameObject == dotButtonTemplate.gameObject)
                continue;

            if (currentIndex == index)
            {
                child.GetComponent<Image>().color = dotSelectedColor;
                child.transform.localScale = Vector3.one * 1.5f;
                currentDotIndex = index;
            }
            else
            {
                child.GetComponent<Image>().color = dotUnselectedColor;
                child.transform.localScale = Vector3.one;
            }

            currentIndex++;
        }
    }

    public void ResetManualCarousel()
    {
        isInTransition = false;
        currentItemIndex = 0;
        SetSelectedDot(0);
    }

    internal IEnumerator RegisterCurrentInstantiatedItems()
    {
        instantiatedItems.Clear();

        foreach (Transform child in itemsContainer)
        {
            BaseComponentView existingItem = child.GetComponent<BaseComponentView>();
            if (existingItem != null)
                instantiatedItems.Add(existingItem);
            else
                Destroy(child.gameObject);
        }

        // In the first loading, before calculating the size of the current items, it is needed to wait for a frame in order to
        // allow time for the carousel viewport to get its final size to be able to execute the 'ResizeAllItems' function correctly.
        yield return null;

        ResizeAllItems();
        SetManualControlsActive(model.showManualControls);
        GenerateDotsSelector();
    }
}
