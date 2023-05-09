using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using static UnityEngine.UI.GridLayoutGroup;

public class GridContainerComponentViewTests
{
    private GridContainerComponentView gridContainerComponent;

    [SetUp]
    public void SetUp() { gridContainerComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<GridContainerComponentView>("GridContainer"); }

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
            itemSize = new Vector2Int(10, 10),
            constraintCount = 3,
            spaceBetweenItems = new Vector2Int(5, 5),
            adaptHorizontallyItemSizeToContainer = false
        };

        // Act
        gridContainerComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, gridContainerComponent.model, "The model does not match.");
    }

    [Test]
    public void SetConstraintCorrectly()
    {
        // Arrange
        Constraint testConstraint = Constraint.FixedColumnCount;

        // Act
        gridContainerComponent.SetConstraint(testConstraint);

        // Assert
        Assert.AreEqual(testConstraint, gridContainerComponent.model.constraint, "The constraint does not match in the model.");
        Assert.AreEqual(testConstraint, gridContainerComponent.gridLayoutGroup.constraint, "The constraint does not match.");
    }

    [Test]
    public void SetConstraintCountCorrectly()
    {
        // Arrange
        int testConstraintCount = 3;

        // Act
        gridContainerComponent.SetConstraintCount(testConstraintCount);

        // Assert
        Assert.AreEqual(testConstraintCount, gridContainerComponent.model.constraintCount, "The constraintCount does not match in the model.");
        Assert.AreEqual(testConstraintCount, gridContainerComponent.gridLayoutGroup.constraintCount, "The constraintCount does not match.");
    }

    [Test]
    public void SetItemSizeToContainerAdaptationCorrectly()
    {
        // Arrange
        bool testadaptItemSizeToContainer = false;

        // Act
        gridContainerComponent.SetItemSizeToContainerAdaptation(testadaptItemSizeToContainer);

        // Assert
        Assert.AreEqual(testadaptItemSizeToContainer, gridContainerComponent.model.adaptHorizontallyItemSizeToContainer, "The adaptItemSizeToContainer does not match in the model.");
    }

    [Test]
    [TestCase(Constraint.FixedColumnCount, true)]
    [TestCase(Constraint.FixedColumnCount, false)]
    [TestCase(Constraint.FixedRowCount, true)]
    [TestCase(Constraint.FixedRowCount, false)]
    [TestCase(Constraint.Flexible, true)]
    [TestCase(Constraint.Flexible, false)]
    public void SetItemSizeCorrectly(Constraint constraint, bool autoItemSize)
    {
        // Arrange
        gridContainerComponent.model.constraint = constraint;
        gridContainerComponent.model.adaptHorizontallyItemSizeToContainer = autoItemSize;
        Vector2 testItemSize = new Vector2(10f, 10f);
        if (autoItemSize)
            ((RectTransform)gridContainerComponent.transform).rect.Set(0, 0, 100, 100);

        List<BaseComponentView> testItems = new List<BaseComponentView>();
        testItems.Add(BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common"));
        gridContainerComponent.SetItems(testItems);

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
    public void SetMinWidthForFlexibleItemsCorrectly()
    {
        // Arrange
        float testMinWidthForFlexibleItems = 200f;

        // Act
        gridContainerComponent.SetMinWidthForFlexibleItems(testMinWidthForFlexibleItems);

        // Assert
        Assert.AreEqual(testMinWidthForFlexibleItems, gridContainerComponent.model.minWidthForFlexibleItems, "The space between items does not match in the model.");
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
        gridContainerComponent.SetItems(testItems);

        // Assert
        Assert.AreEqual(testItems.Count, gridContainerComponent.transform.childCount, "The number of items list does not match.");
        Assert.IsTrue(gridContainerComponent.instantiatedItems.Contains(testItems[0]), "The item 1 does not exist in the instantiatedItems list.");
        Assert.IsTrue(gridContainerComponent.instantiatedItems.Contains(testItems[1]), "The item 2 does not exist in the instantiatedItems list.");
        Assert.IsTrue(gridContainerComponent.instantiatedItems.Contains(testItems[2]), "The item 3 does not exist in the instantiatedItems list.");
    }

    [Test]
    public void AddItemCorrectly()
    {
        // Arrange
        BaseComponentView testItem = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");

        // Act
        gridContainerComponent.AddItemWithResize(testItem);

        // Assert
        Assert.IsTrue(gridContainerComponent.instantiatedItems.Contains(testItem), "The item does not exist in the instantiatedItems list.");
    }

    [Test]
    public void RemoveItemCorrectly()
    {
        // Arrange
        BaseComponentView testItem = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        gridContainerComponent.AddItemWithResize(testItem);

        // Act
        gridContainerComponent.RemoveItem(testItem);

        // Assert
        Assert.IsFalse(gridContainerComponent.instantiatedItems.Contains(testItem), "The item still exists in the instantiatedItems list.");
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
        gridContainerComponent.SetItems(testItems);

        // Act
        List<BaseComponentView> allExistingItems = gridContainerComponent.GetItems();

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
        gridContainerComponent.SetItems(testItems);

        // Act
        List<BaseComponentView> allExtractedItems = gridContainerComponent.ExtractItems();

        // Assert
        Assert.AreEqual(testItems.Count, allExtractedItems.Count, "The number of items gotten do not match.");
        Assert.AreEqual(allExtractedItems[0], testItems[0], "The item 1 extracted does not match.");
        Assert.AreEqual(allExtractedItems[1], testItems[1], "The item 2 extracted does not match.");
        Assert.IsTrue(allExtractedItems[0].transform.parent == null, "The parent of the item 1 extracted is not null.");
        Assert.IsTrue(allExtractedItems[1].transform.parent == null, "The parent of the item 1 extracted is not null.");
        Assert.IsTrue(gridContainerComponent.instantiatedItems.Count == 0, "The instantiated items list is not empty.");

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
        gridContainerComponent.SetItems(testItems);

        // Act
        gridContainerComponent.RemoveItems();
        yield return null;

        // Assert
        Assert.AreEqual(0, gridContainerComponent.transform.childCount, "The number of items list does not match.");
    }

    [Test]
    public void CreateItemCorrectly()
    {
        // Arrange
        ButtonComponentView testItem = BaseComponentView.CreateUIComponentFromAssetDatabase<ButtonComponentView>("Button_Common");
        string testName = "TestName";

        // Act
        gridContainerComponent.CreateItem(testItem, testName);

        // Assert
        Assert.AreEqual(gridContainerComponent.transform, testItem.transform.parent, "The parent of the item should be the own grid.");
        Assert.AreEqual(Vector3.zero, testItem.transform.localPosition, "The item position should be 0.");
        Assert.AreEqual(Vector3.one, testItem.transform.localScale, "The item position should be 1.");
        Assert.AreEqual(testName, testItem.name, "The item name does not match.");
        Assert.IsTrue(gridContainerComponent.instantiatedItems.Contains(testItem), "The item does not exist in the instantiatedItems list.");
    }
}
