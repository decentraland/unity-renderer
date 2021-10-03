using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ICarouselComponentView
{
    /// <summary>
    /// Fill the model and updates the carousel with this data.
    /// </summary>
    /// <param name="model">Data to configure the carousel.</param>
    void Configure(CarouselComponentModel model);

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
    /// <param name="instantiateNewCopyOfItems">Indicates if the items provided will be instantiated as a new copy or not.</param>
    void SetItems(List<BaseComponentView> items, bool instantiateNewCopyOfItems = true);

    /// <summary>
    /// Get an item of the carousel.
    /// </summary>
    /// <param name="index">Index of the list of items.</param>
    /// <returns>A specific UI component.</returns>
    BaseComponentView GetItem(int index);

    /// <summary>
    /// Get all the items of the carousel.
    /// </summary>
    /// <returns>The list of items.</returns>
    List<BaseComponentView> GetAllItems();

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

public class CarouselComponentView : BaseComponentView, ICarouselComponentView
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
    internal bool destroyOnlyUnnecesaryItems = false;

    public override void PostInitialization()
    {
        Configure(model);
        ConfigureManualButtonsEvents();
        StartCarousel();
    }

    public void Configure(CarouselComponentModel model)
    {
        this.model = model;
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
        SetItems(model.items);
    }

    public override void Dispose()
    {
        base.Dispose();

        StopCarousel();
        DestroyInstantiatedItems(true);
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

        previousButton.gameObject.SetActive(isActived);
        nextButton.gameObject.SetActive(isActived);
    }

    public void SetItems(List<BaseComponentView> items, bool instantiateNewCopyOfItems = true)
    {
        model.items = items;

        DestroyInstantiatedItems(!destroyOnlyUnnecesaryItems);

        for (int i = 0; i < items.Count; i++)
        {
            CreateItem(items[i], $"Item{i}", instantiateNewCopyOfItems && !destroyOnlyUnnecesaryItems);
        }

        if (!instantiateNewCopyOfItems)
            destroyOnlyUnnecesaryItems = true;
    }

    public BaseComponentView GetItem(int index)
    {
        if (index >= instantiatedItems.Count)
            return null;

        return instantiatedItems[index];
    }

    public List<BaseComponentView> GetAllItems() { return instantiatedItems; }

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

    internal void CreateItem(BaseComponentView newItem, string name, bool instantiateNewCopyOfItem = true)
    {
        if (Application.isPlaying)
        {
            InstantiateItem(newItem, name, instantiateNewCopyOfItem);
        }
        else
        {
            if (isActiveAndEnabled)
                StartCoroutine(IntantiateItemOnEditor(newItem, name));
        }
    }

    internal void InstantiateItem(BaseComponentView newItem, string name, bool instantiateNewCopyOfItem = true)
    {
        if (newItem == null)
            return;

        BaseComponentView newGO;
        if (instantiateNewCopyOfItem)
        {
            newGO = Instantiate(newItem, itemsContainer);
        }
        else
        {
            newGO = newItem;
            newGO.transform.SetParent(itemsContainer);
            newGO.transform.localPosition = Vector3.zero;
            newGO.transform.localScale = Vector3.one;
        }

        newGO.name = name;
        ((RectTransform)newGO.transform).sizeDelta = new Vector2(viewport.rect.width, viewport.rect.height);

        instantiatedItems.Add(newGO);

        itemsContainer.offsetMin = Vector2.zero;
        if (instantiatedItems.Count > 1)
        {
            float extraSpace = (instantiatedItems.Count - 1) * model.spaceBetweenItems;
            itemsContainer.offsetMax = new Vector2(viewport.rect.width * (instantiatedItems.Count - 1) + extraSpace, 0);
        }
    }

    internal IEnumerator IntantiateItemOnEditor(BaseComponentView newItem, string name)
    {
        yield return null;
        InstantiateItem(newItem, name);
    }

    internal void DestroyInstantiatedItems(bool forzeToDestroyAll)
    {
        if (forzeToDestroyAll)
        {
            foreach (Transform child in itemsContainer)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    if (isActiveAndEnabled)
                        StartCoroutine(DestroyGameObjectOnEditor(child.gameObject));
                }
            }
        }
        else
        {
            foreach (BaseComponentView child in instantiatedItems)
            {
                if (!model.items.Contains(child))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        instantiatedItems.Clear();

        itemsContainer.offsetMin = Vector2.zero;
        itemsContainer.offsetMax = Vector2.zero;
    }

    internal IEnumerator DestroyGameObjectOnEditor(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }

    internal IEnumerator RunCarouselCoroutine(
        int fromIndex = 0,
        bool startInmediately = false,
        CarouselDirection direction = CarouselDirection.Right,
        bool changeDirectionAfterFirstTransition = false)
    {
        currentItemIndex = fromIndex;

        while (true)
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
        if (currentItemIndex == instantiatedItems.Count - 1)
        {
            currentItemIndex = 1;
            itemsScroll.horizontalNormalizedPosition = 0f;
            itemsContainer.GetChild(itemsContainer.childCount - 1).SetAsFirstSibling();

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
                itemsContainer.GetChild(itemsContainer.childCount - 1).SetAsFirstSibling();
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
}