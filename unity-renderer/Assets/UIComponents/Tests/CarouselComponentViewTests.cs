using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class CarouselComponentViewTests
{
    private CarouselComponentView carouselComponent;

    [SetUp]
    public void SetUp() { carouselComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<CarouselComponentView>("Carousel"); }

    [TearDown]
    public void TearDown()
    {
        carouselComponent.Dispose();
        GameObject.Destroy(carouselComponent.gameObject);
    }

    [Test]
    public void ConfigureCarouselCorrectly()
    {
        // Arrange
        CarouselComponentModel testModel = new CarouselComponentModel
        {
            animationCurve = new AnimationCurve(),
            animationTransitionTime = 1f,
            backgroundColor = Color.black,
            showManualControls = true,
            spaceBetweenItems = 10f,
            timeBetweenItems = 10f
        };

        // Act
        carouselComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, carouselComponent.model, "The model does not match.");
    }

    [Test]
    public void SetSpaceBetweenItemsCorrectly()
    {
        // Arrange
        float testSpaceBetweenItems = 10f;

        // Act
        carouselComponent.SetSpaceBetweenItems(testSpaceBetweenItems);

        // Assert
        Assert.AreEqual(testSpaceBetweenItems, carouselComponent.model.spaceBetweenItems, "The space between items does not match in the model.");
        Assert.AreEqual(testSpaceBetweenItems, carouselComponent.horizontalLayout.spacing, "The space between items does not match.");
    }

    [Test]
    public void SetTimeBetweenItemsCorrectly()
    {
        // Arrange
        float testTimeBetweenItems = 1f;

        // Act
        carouselComponent.SetTimeBetweenItems(testTimeBetweenItems);

        // Assert
        Assert.AreEqual(testTimeBetweenItems, carouselComponent.model.timeBetweenItems, "The time between items does not match in the model.");
    }

    [Test]
    public void SetAnimationTransitionTimeCorrectly()
    {
        // Arrange
        float testAnimationTransitionTime = 1f;

        // Act
        carouselComponent.SetAnimationTransitionTime(testAnimationTransitionTime);

        // Assert
        Assert.AreEqual(testAnimationTransitionTime, carouselComponent.model.animationTransitionTime, "The animation transition time does not match in the model.");
    }

    [Test]
    public void SetAnimationCurveCorrectly()
    {
        // Arrange
        AnimationCurve testAnimationCurve = new AnimationCurve();

        // Act
        carouselComponent.SetAnimationCurve(testAnimationCurve);

        // Assert
        Assert.AreEqual(testAnimationCurve, carouselComponent.model.animationCurve, "The animation curve does not match in the model.");
    }

    [Test]
    public void SetBackgroundColorCorrectly()
    {
        // Arrange
        Color testColor = Color.black;

        // Act
        carouselComponent.SetBackgroundColor(testColor);

        // Assert
        Assert.AreEqual(testColor, carouselComponent.model.backgroundColor, "The background color does not match in the model.");

        if (carouselComponent.background != null)
            Assert.AreEqual(testColor, carouselComponent.background.color, "The background color does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetManualControlsActiveCorrectly(bool isActived)
    {
        // Arrange
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.SetManualControlsActive(isActived);

        // Assert
        Assert.AreEqual(isActived, carouselComponent.model.showManualControls, "The showManualControls does not match in the model.");
        Assert.AreEqual(isActived, carouselComponent.previousButton.gameObject.activeSelf, "The previous button active property does not match.");
        Assert.AreEqual(isActived, carouselComponent.nextButton.gameObject.activeSelf, "The next button active property does not match.");
        Assert.AreEqual(isActived, carouselComponent.dotsSelector.gameObject.activeSelf, "The dots selector active property does not match.");
    }

    [Test]
    public void SetItemsCorrectly()
    {
        // Arrange
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common"));
        testItems.Add(BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common"));
        testItems.Add(BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common"));

        // Act
        carouselComponent.SetItems(testItems);

        // Assert
        Assert.AreEqual(testItems.Count, carouselComponent.itemsContainer.transform.childCount, "The number of items list does not match.");
        Assert.IsTrue(carouselComponent.instantiatedItems.Contains(testItems[0]), "The item 1 does not exist in the instantiatedItems list.");
        Assert.IsTrue(carouselComponent.instantiatedItems.Contains(testItems[1]), "The item 2 does not exist in the instantiatedItems list.");
        Assert.IsTrue(carouselComponent.instantiatedItems.Contains(testItems[2]), "The item 3 does not exist in the instantiatedItems list.");
    }

    [Test]
    public void AddItemCorrectly()
    {
        // Arrange
        BaseComponentView testItem = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");

        // Act
        carouselComponent.AddItemWithDotsSelector(testItem);

        // Assert
        Assert.IsTrue(carouselComponent.instantiatedItems.Contains(testItem), "The item does not exist in the instantiatedItems list.");
    }

    [Test]
    public void RemoveItemCorrectly()
    {
        // Arrange
        BaseComponentView testItem = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        carouselComponent.AddItemWithDotsSelector(testItem);

        // Act
        carouselComponent.RemoveItem(testItem);

        // Assert
        Assert.IsFalse(carouselComponent.instantiatedItems.Contains(testItem), "The item still exists in the instantiatedItems list.");
    }

    [Test]
    public void GetItemsCorrectly()
    {
        // Arrange
        ButtonComponentView testItem1 = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        ImageComponentView testItem2 = BaseComponentView.CreateUIComponentFromAssetDatabase<ImageComponentView>("Image");
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(testItem1);
        testItems.Add(testItem2);
        carouselComponent.SetItems(testItems);

        // Act
        List<BaseComponentView> allExistingItems = carouselComponent.GetItems();

        // Assert
        Assert.AreEqual(testItems.Count, allExistingItems.Count, "The number of items gotten do not match.");
        Assert.AreEqual(allExistingItems[0], testItems[0], "The item 1 gotten does not match.");
        Assert.AreEqual(allExistingItems[1], testItems[1], "The item 2 gotten does not match.");
    }

    [Test]
    public void ExtractItemsCorrectly()
    {
        // Arrange
        ButtonComponentView testItem1 = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        ImageComponentView testItem2 = BaseComponentView.CreateUIComponentFromAssetDatabase<ImageComponentView>("Image");
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(testItem1);
        testItems.Add(testItem2);
        carouselComponent.SetItems(testItems);

        // Act
        List<BaseComponentView> allExtractedItems = carouselComponent.ExtractItems();

        // Assert
        Assert.AreEqual(testItems.Count, allExtractedItems.Count, "The number of items gotten do not match.");
        Assert.AreEqual(allExtractedItems[0], testItems[0], "The item 1 extracted does not match.");
        Assert.AreEqual(allExtractedItems[1], testItems[1], "The item 2 extracted does not match.");
        Assert.IsTrue(allExtractedItems[0].transform.parent == null, "The parent of the item 1 extracted is not null.");
        Assert.IsTrue(allExtractedItems[1].transform.parent == null, "The parent of the item 1 extracted is not null.");
        Assert.IsTrue(carouselComponent.instantiatedItems.Count == 0, "The instantiated items list is not empty.");

        Object.Destroy(testItem1.gameObject);
        Object.Destroy(testItem2.gameObject);
    }

    [UnityTest]
    public IEnumerator RemoveItemsCorrectly()
    {
        // Arrange
        ButtonComponentView testItem1 = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        ImageComponentView testItem2 = BaseComponentView.CreateUIComponentFromAssetDatabase<ImageComponentView>("Image");
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(testItem1);
        testItems.Add(testItem2);
        carouselComponent.SetItems(testItems);

        // Act
        carouselComponent.RemoveItems();
        yield return null;

        // Assert
        Assert.AreEqual(0, carouselComponent.itemsContainer.transform.childCount, "The number of items list does not match.");
    }

    [Test]
    public void StartCarouselCorrectly()
    {
        // Arrange
        carouselComponent.itemsCoroutine = null;
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.StartCarousel();

        // Assert
        Assert.IsNotNull(carouselComponent.itemsCoroutine, "The carousel coroutine is null.");
    }

    [Test]
    public void StopCarouselCorrectly()
    {
        // Arrange
        carouselComponent.itemsCoroutine = null;
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.StopCarousel();

        // Assert
        Assert.IsNull(carouselComponent.itemsCoroutine, "The carousel coroutine is not null.");
        Assert.IsFalse(carouselComponent.isInTransition, "The isInTransition flag should be false.");
    }

    [Test]
    public void GoToPreviousItemCorrectly()
    {
        // Arrange
        carouselComponent.itemsCoroutine = null;
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.GoToPreviousItem();

        // Assert
        Assert.IsNotNull(carouselComponent.itemsCoroutine, "The carousel coroutine is null.");
    }

    [Test]
    public void GoToNextItemCorrectly()
    {
        // Arrange
        carouselComponent.itemsCoroutine = null;
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.GoToNextItem();

        // Assert
        Assert.IsNotNull(carouselComponent.itemsCoroutine, "The carousel coroutine is null.");
    }

    [Test]
    public void MakeJumpFromDotsSelectorCorrectly()
    {
        // Arrange
        carouselComponent.itemsCoroutine = null;
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.MakeJumpFromDotsSelector(1, CarouselDirection.Right);

        // Assert
        Assert.IsNotNull(carouselComponent.itemsCoroutine, "The carousel coroutine is null.");
    }

    [Test]
    [TestCase("PreviousButton")]
    [TestCase("NextButton")]
    public void ConfigureManualButtonsEventsCorrectly(string manualButtonToTest)
    {
        // Arrange
        carouselComponent.itemsCoroutine = null;
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.ConfigureManualButtonsEvents();

        // Assert
        if (manualButtonToTest == "PreviousButton")
            carouselComponent.previousButton.onClick.Invoke();
        else
            carouselComponent.nextButton.onClick.Invoke();

        Assert.IsNotNull(carouselComponent.itemsCoroutine, "The carousel coroutine is null.");
    }

    [Test]
    public void CreateItemCorrectly()
    {
        // Arrange
        ButtonComponentView testItem = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        string testName = "TestName";

        // Act
        carouselComponent.CreateItem(testItem, testName);

        // Assert
        Assert.AreEqual(carouselComponent.itemsContainer.transform, testItem.transform.parent, "The parent of the item should be the own grid.");
        Assert.AreEqual(Vector3.one, testItem.transform.localScale, "The item position should be 1.");
        Assert.AreEqual(testName, testItem.name, "The item name does not match.");
        Assert.IsTrue(carouselComponent.instantiatedItems.Contains(testItem), "The item does not exist in the instantiatedItems list.");
    }

    [UnityTest]
    public IEnumerator GenerateDotsSelectorCorrectly()
    {
        // Arrange
        FillCarouselWithSomeTestItems();

        // Act
        carouselComponent.GenerateDotsSelector();
        yield return null;

        // Assert
        Assert.AreEqual(3, carouselComponent.dotsSelector.transform.childCount, "The number of dots does not match.");
        Assert.AreEqual(0, carouselComponent.currentDotIndex, "The currentDotIndex should be 0.");
    }

    [UnityTest]
    public IEnumerator SetSelectedDotCorrectly()
    {
        // Arrange
        int testDotIndex = 1;
        yield return GenerateDotsSelectorCorrectly();

        // Act
        carouselComponent.SetSelectedDot(testDotIndex);

        // Assert
        Assert.AreEqual(testDotIndex, carouselComponent.currentDotIndex, "The currentDotIndex does not match.");
    }

    private void FillCarouselWithSomeTestItems()
    {
        ButtonComponentView testItem1 = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        ImageComponentView testItem2 = BaseComponentView.CreateUIComponentFromAssetDatabase<ImageComponentView>("Image");
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(testItem1);
        testItems.Add(testItem2);
        carouselComponent.SetItems(testItems);
    }
}
