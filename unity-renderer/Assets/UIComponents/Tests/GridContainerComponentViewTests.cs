using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class GridContainerComponentViewTests
{
    private GridContainerComponentView gridContainerComponent;

    [SetUp]
    public void SetUp() { gridContainerComponent = BaseComponentView.Create<GridContainerComponentView>("GridContainer"); }

    [TearDown]
    public void TearDown()
    {
        gridContainerComponent.Dispose();
        GameObject.Destroy(gridContainerComponent.gameObject);
    }

    [Test]
    public void ConfigureGridContainerCorrectly()
    {
        // Arrange
        GridContainerComponentModel testModel = new GridContainerComponentModel
        {
            items = new List<BaseComponentView>(),
            itemSize = new Vector2Int(10, 10),
            constranitCount = 3,
            spaceBetweenItems = new Vector2Int(5, 5),
            adaptItemSizeToGridSize = false
        };

        // Act
        gridContainerComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, gridContainerComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void RefreshGridContainerCorrectly()
    {
        // Arrange
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        Vector2 testItemSize = new Vector2(10f, 10f);
        int testNumColumns = 3;
        Vector2 testSpaceBetweenItems = new Vector2(5f, 5f);

        gridContainerComponent.model.items = testItems;
        gridContainerComponent.model.itemSize = testItemSize;
        gridContainerComponent.model.constranitCount = testNumColumns;
        gridContainerComponent.model.spaceBetweenItems = testSpaceBetweenItems;
        gridContainerComponent.model.adaptItemSizeToGridSize = false;

        // Act
        gridContainerComponent.RefreshControl();

        // Assert
        Assert.AreEqual(testItems, gridContainerComponent.model.items, "The items does not match in the model.");
        Assert.AreEqual(testItemSize, gridContainerComponent.model.itemSize, "The item size does not match in the model.");
        Assert.AreEqual(testNumColumns, gridContainerComponent.model.constranitCount, "The number of columns does not match in the model.");
        Assert.AreEqual(testSpaceBetweenItems, gridContainerComponent.model.spaceBetweenItems, "The space between items does not match in the model.");
    }

    [Test]
    public void SetNumberOfColumnsCorrectly()
    {
        // Arrange
        int testNumColumns = 3;

        // Act
        gridContainerComponent.SetConstraintCount(testNumColumns);

        // Assert
        Assert.AreEqual(testNumColumns, gridContainerComponent.model.constranitCount, "The number of columns does not match in the model.");
        Assert.AreEqual(testNumColumns, gridContainerComponent.gridLayoutGroup.constraintCount, "The number of columns does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetItemSizeCorrectly(bool autoItemSize)
    {
        // Arrange
        gridContainerComponent.model.adaptItemSizeToGridSize = autoItemSize;
        gridContainerComponent.model.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        Vector2 testItemSize = new Vector2(10f, 10f);
        if (autoItemSize)
            ((RectTransform)gridContainerComponent.transform).rect.Set(0, 0, 100, 100);

        // Act
        gridContainerComponent.SetItemSize(testItemSize);

        // Assert
        if (autoItemSize)
        {
            Assert.AreNotEqual(testItemSize, gridContainerComponent.model.itemSize, "The item size does not match in the model.");
            Assert.AreNotEqual(testItemSize, gridContainerComponent.gridLayoutGroup.cellSize, "The item size does not match.");
        }
        else
        {
            Assert.AreEqual(testItemSize, gridContainerComponent.model.itemSize, "The item size does not match in the model.");
            Assert.AreEqual(testItemSize, gridContainerComponent.gridLayoutGroup.cellSize, "The item size does not match.");
        }
    }

    [Test]
    public void SetSpaceBetweenItemsCorrectly()
    {
        // Arrange
        Vector2 testSpaceBetweenItems = new Vector2(5f, 5f);

        // Act
        gridContainerComponent.SetSpaceBetweenItems(testSpaceBetweenItems);

        // Assert
        Assert.AreEqual(testSpaceBetweenItems, gridContainerComponent.model.spaceBetweenItems, "The space between items does not match in the model.");
        Assert.AreEqual(testSpaceBetweenItems, gridContainerComponent.gridLayoutGroup.spacing, "The space between items does not match.");
    }

    [Test]
    public void SetItemsCorrectly()
    {
        // Arrange
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(BaseComponentView.Create<ButtonComponentView>("Button_Common"));
        testItems.Add(BaseComponentView.Create<ButtonComponentView>("Button_Common"));
        testItems.Add(BaseComponentView.Create<ButtonComponentView>("Button_Common"));

        // Act
        gridContainerComponent.SetItems(testItems);

        // Assert
        Assert.AreEqual(testItems, gridContainerComponent.model.items, "The items list does not match in the model.");
        Assert.AreEqual(testItems.Count, gridContainerComponent.transform.childCount, "The number of items list does not match.");
    }

    [Test]
    public void GetItemCorrectly()
    {
        // Arrange
        ButtonComponentView testItem1 = BaseComponentView.Create<ButtonComponentView>("Button_Common");
        ImageComponentView testItem2 = BaseComponentView.Create<ImageComponentView>("Image");
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(testItem1);
        testItems.Add(testItem2);
        gridContainerComponent.SetItems(testItems);

        // Act
        BaseComponentView existingItem1 = gridContainerComponent.GetItem(0);
        BaseComponentView existingItem2 = gridContainerComponent.GetItem(1);

        // Assert
        Assert.IsTrue(existingItem1 is ButtonComponentView, "The item 1 gotten does not match.");
        Assert.IsTrue(existingItem2 is ImageComponentView, "The item 2 gotten does not match.");
    }

    [Test]
    public void GetAllItemsCorrectly()
    {
        // Arrange
        ButtonComponentView testItem1 = BaseComponentView.Create<ButtonComponentView>("Button_Common");
        ImageComponentView testItem2 = BaseComponentView.Create<ImageComponentView>("Image");
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(testItem1);
        testItems.Add(testItem2);
        gridContainerComponent.SetItems(testItems);

        // Act
        List<BaseComponentView> allExistingItems = gridContainerComponent.GetAllItems();

        // Assert
        Assert.IsTrue(allExistingItems[0] is ButtonComponentView, "The item 1 gotten does not match.");
        Assert.IsTrue(allExistingItems[1] is ImageComponentView, "The item 2 gotten does not match.");
        Assert.AreEqual(testItems.Count, allExistingItems.Count, "The number of items gotten do not match.");
    }

    [UnityTest]
    public IEnumerator RemoveAllInstantiatedItemsCorrectly()
    {
        // Arrange
        ButtonComponentView testItem1 = BaseComponentView.Create<ButtonComponentView>("Button_Common");
        ImageComponentView testItem2 = BaseComponentView.Create<ImageComponentView>("Image");
        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(testItem1);
        testItems.Add(testItem2);
        gridContainerComponent.SetItems(testItems);

        // Act
        gridContainerComponent.DestroyInstantiatedItems(true);
        yield return null;

        // Assert
        Assert.AreEqual(0, gridContainerComponent.transform.childCount, "The number of items list does not match.");
    }
}