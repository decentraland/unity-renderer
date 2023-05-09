using NUnit.Framework;
using UnityEngine;

public class ColumnsOrganizerComponentViewTests
{
    private ColumnsOrganizerComponentView columnsOrganizerComponent;

    [SetUp]
    public void SetUp() { columnsOrganizerComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<ColumnsOrganizerComponentView>("ColumnsOrganizer"); }

    [TearDown]
    public void TearDown()
    {
        columnsOrganizerComponent.Dispose();
        GameObject.Destroy(columnsOrganizerComponent.gameObject);
    }

    [Test]
    public void RecalculateColumnsSizeCorrectly()
    {
        // Arrange
        float testContainerWidth = 100f;
        columnsOrganizerComponent.columnsContainer.sizeDelta = new Vector2(testContainerWidth, 10);

        GameObject testColumn1 = GameObject.Instantiate(new GameObject(), columnsOrganizerComponent.transform);
        testColumn1.AddComponent<RectTransform>();
        (testColumn1.transform as RectTransform).sizeDelta = new Vector2(0, 10);
        ColumnConfigModel testColumnConfig1 = new ColumnConfigModel
        {
            isPercentage = false,
            width = 20
        };
        columnsOrganizerComponent.model.columnsConfig.Add(testColumnConfig1);

        GameObject testColumn2 = GameObject.Instantiate(new GameObject(), columnsOrganizerComponent.transform);
        testColumn2.AddComponent<RectTransform>();
        (testColumn2.transform as RectTransform).sizeDelta = new Vector2(0, 10);
        ColumnConfigModel testColumnConfig2 = new ColumnConfigModel
        {
            isPercentage = false,
            width = 30
        };
        columnsOrganizerComponent.model.columnsConfig.Add(testColumnConfig2);

        GameObject testColumn3 = GameObject.Instantiate(new GameObject(), columnsOrganizerComponent.transform);
        testColumn3.AddComponent<RectTransform>();
        (testColumn3.transform as RectTransform).sizeDelta = new Vector2(0, 10);
        ColumnConfigModel testColumnConfig3 = new ColumnConfigModel
        {
            isPercentage = true,
            width = 100
        };
        columnsOrganizerComponent.model.columnsConfig.Add(testColumnConfig3);

        // Act
        columnsOrganizerComponent.RecalculateColumnsSize();

        // Assert
        Assert.AreEqual(testColumnConfig1.width, (testColumn1.transform as RectTransform).sizeDelta.x);
        Assert.AreEqual(testColumnConfig2.width, (testColumn2.transform as RectTransform).sizeDelta.x);
        Assert.AreEqual(testContainerWidth - testColumnConfig1.width - testColumnConfig2.width, (testColumn3.transform as RectTransform).sizeDelta.x);
    }
}
