using System.Collections;
using System.Collections.Generic;
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
    void StartCarousel(int fromIndex, bool startInmediately, CarouselDirection direction, bool changeDirectionAfterFirstTransition);

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
}

public enum CarouselDirection
{
    Right,
    Left
}

public class CarouselComponentView : BaseComponentView, ICarouselComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal RectTransform itemsContainer;
    [SerializeField] internal HorizontalLayoutGroup horizontalLayout;
    [SerializeField] internal ScrollRect itemsScroll;
    [SerializeField] internal RectTransform viewport;
    [SerializeField] internal Image background;
    [SerializeField] internal Button previousButton;
    [SerializeField] internal Button nextButton;

    [Header("Configuration")]
    [SerializeField] internal CarouselComponentModel model;

    internal List<BaseComponentView> instantiatedItems = new List<BaseComponentView>();
    internal Coroutine itemsCoroutine;
    internal int currentItemIndex = 0;
    internal float currentFinalNormalizedPos;
    internal bool isInTransition = false;

    public override void Initialization()
    {
        base.Initialization();

        StartCoroutine(RegisterCurrentInstantiatedItems());
        ConfigureManualButtonsEvents();
    }

    public override void PostInitialization() { StartCarousel(); }

    public void Configure(BaseComponentModel newModel)
    {
        model = (CarouselComponentModel)newModel;
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
    }

    public override void PostScreenSizeChanged()
    {
        base.PostScreenSizeChanged();

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

    public void SetTimeBetweenItems(float newTime) { model.timeBetweenItems = newTime; }

    public void SetAnimationTransitionTime(float newTime) { model.animationTransitionTime = newTime; }

    public void SetAnimationCurve(AnimationCurve newCurve) { model.animationCurve = newCurve; }

    public void SetBackgroundColor(Color newColor)
    {
        model.backgroundColor = newColor;

        if (background == null)
            return;

        background.color = newColor;
    }

    public void SetManualControlsActive(bool isActived)
    {
        model.showManualControls = isActived;

        if (previousButton == null || nextButton == null)
            return;

        int currentNumberOfItems = itemsContainer.childCount;
        previousButton.gameObject.SetActive(isActived && currentNumberOfItems > 1);
        nextButton.gameObject.SetActive(isActived && currentNumberOfItems > 1);
    }

    public void SetItems(List<BaseComponentView> items)
    {
        DestroyInstantiatedItems();

        for (int i = 0; i < items.Count; i++)
        {
            CreateItem(items[i], $"Item{i}");
        }

        SetManualControlsActive(model.showManualControls);
    }

    public List<BaseComponentView> GetItems() { return instantiatedItems; }

    public List<BaseComponentView> ExtractItems()
    {
        List<BaseComponentView> extractedItems = new List<BaseComponentView>();
        foreach (BaseComponentView item in instantiatedItems)
        {
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
        bool changeDirectionAfterFirstTransition = false)
    {
        itemsCoroutine = CoroutineStarter.Start(RunCarouselCoroutine(fromIndex, startInmediately, direction, changeDirectionAfterFirstTransition));
    }

    public void StopCarousel()
    {
        if (itemsCoroutine == null)
            return;

        CoroutineStarter.Stop(itemsCoroutine);
        itemsCoroutine = null;
    }

    public void GoToPreviousItem()
    {
        if (isInTransition)
            return;

        StopCarousel();
        StartCarousel(
            fromIndex: currentItemIndex,
            startInmediately: true,
            direction: CarouselDirection.Left,
            changeDirectionAfterFirstTransition: true);
    }

    public void GoToNextItem()
    {
        if (isInTransition)
            return;

        StopCarousel();
        StartCarousel(
            fromIndex: currentItemIndex,
            startInmediately: true,
            direction: CarouselDirection.Right,
            changeDirectionAfterFirstTransition: false);
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
        foreach (Transform child in itemsContainer)
        {
            ResizeItem((RectTransform)child);
        }
    }

    internal void DestroyInstantiatedItems()
    {
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        instantiatedItems.Clear();

        itemsContainer.offsetMin = Vector2.zero;
        itemsContainer.offsetMax = Vector2.zero;
    }

    internal IEnumerator RunCarouselCoroutine(
        int fromIndex = 0,
        bool startInmediately = false,
        CarouselDirection direction = CarouselDirection.Right,
        bool changeDirectionAfterFirstTransition = false)
    {
        currentItemIndex = fromIndex;

        while (itemsContainer.childCount > 1)
        {
            if (!startInmediately)
                yield return new WaitForSeconds(model.timeBetweenItems);
            else
                startInmediately = false;

            if (instantiatedItems.Count > 0)
            {
                if (direction == CarouselDirection.Right)
                {
                    yield return RunRightAnimation();

                    if (changeDirectionAfterFirstTransition)
                    {
                        direction = CarouselDirection.Left;
                        changeDirectionAfterFirstTransition = false;
                    }
                }
                else
                {
                    yield return RunLeftAnimation();

                    if (changeDirectionAfterFirstTransition)
                    {
                        direction = CarouselDirection.Right;
                        changeDirectionAfterFirstTransition = false;
                    }
                }
            }
        }
    }

    internal IEnumerator RunRightAnimation()
    {
        int currentNumberOfItems = itemsContainer.childCount;

        if (currentItemIndex == instantiatedItems.Count - 1)
        {
            currentItemIndex = 1;
            itemsScroll.horizontalNormalizedPosition = 0f;
            itemsContainer.GetChild(currentNumberOfItems - 1).SetAsFirstSibling();

            yield return RunAnimationCoroutine(CarouselDirection.Right);
        }
        else
        {
            yield return RunAnimationCoroutine(CarouselDirection.Right);

            currentItemIndex++;
            if (currentItemIndex >= instantiatedItems.Count - 1)
            {
                currentItemIndex = 0;
                itemsScroll.horizontalNormalizedPosition = 0f;
                itemsContainer.GetChild(currentNumberOfItems - 1).SetAsFirstSibling();
            }
        }
    }

    internal IEnumerator RunLeftAnimation()
    {
        if (currentItemIndex == 0)
        {
            currentItemIndex = instantiatedItems.Count - 2;
            itemsScroll.horizontalNormalizedPosition = 1f;
            itemsContainer.GetChild(0).SetAsLastSibling();

            yield return RunAnimationCoroutine(CarouselDirection.Left);
        }
        else
        {
            yield return RunAnimationCoroutine(CarouselDirection.Left);

            currentItemIndex--;
            if (currentItemIndex <= 0)
            {
                currentItemIndex = instantiatedItems.Count - 1;
                itemsScroll.horizontalNormalizedPosition = 1f;
                itemsContainer.GetChild(0).SetAsLastSibling();
            }
        }
    }

    internal IEnumerator RunAnimationCoroutine(CarouselDirection direction)
    {
        isInTransition = true;
        float currentAnimationTime = 0f;
        float initialNormalizedPos = itemsScroll.horizontalNormalizedPosition;

        if (direction == CarouselDirection.Right)
            currentFinalNormalizedPos = initialNormalizedPos + (1f / (instantiatedItems.Count - 1));
        else
            currentFinalNormalizedPos = initialNormalizedPos - (1f / (instantiatedItems.Count - 1));

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
    }
}