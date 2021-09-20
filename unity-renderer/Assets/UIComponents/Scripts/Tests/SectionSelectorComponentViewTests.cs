using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class SectionSelectorComponentViewTests
{
    private SectionSelectorComponentView sectionSelectorComponent;

    [SetUp]
    public void SetUp() { sectionSelectorComponent = BaseComponentView.Create<SectionSelectorComponentView>("SectionSelector"); }

    [TearDown]
    public void TearDown()
    {
        sectionSelectorComponent.Dispose();
        GameObject.Destroy(sectionSelectorComponent.gameObject);
    }

    [Test]
    public void ConfigureSectionSelectorCorrectly()
    {
        // Arrange
        SectionSelectorComponentModel testModel = CreateTestModel(3);

        // Act
        sectionSelectorComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, sectionSelectorComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void RefreshSectionSelectorCorrectly()
    {
        // Arrange
        sectionSelectorComponent.model = CreateTestModel(3);
        sectionSelectorComponent.model.sections.Add(new SectionToggleModel
        {
            icon = Sprite.Create(new Texture2D(20, 20), new Rect(), Vector2.zero),
            title = "Test4",
            onSelectEvent = new Toggle.ToggleEvent()
        });

        // Act
        sectionSelectorComponent.RefreshControl();

        // Assert
        Assert.AreEqual(4, sectionSelectorComponent.model.sections.Count, "The number of sections do not match.");
        Assert.AreEqual($"Test1", sectionSelectorComponent.model.sections[0].title, "The section name 1 does not match.");
        Assert.AreEqual($"Test2", sectionSelectorComponent.model.sections[1].title, "The section name 2 does not match.");
        Assert.AreEqual($"Test3", sectionSelectorComponent.model.sections[2].title, "The section name 3 does not match.");
        Assert.AreEqual($"Test4", sectionSelectorComponent.model.sections[3].title, "The section name 4 does not match.");
    }

    [Test]
    public void SetSectionsCorrectly()
    {
        // Arrange
        List<SectionToggleModel> testSections = CreateTestSections(3);

        // Act
        sectionSelectorComponent.SetSections(testSections);

        // Assert
        Assert.AreEqual(testSections, sectionSelectorComponent.model.sections, "The section list does not match in the model.");
        Assert.AreEqual(testSections.Count + 1, sectionSelectorComponent.transform.childCount, "The number of instantiated sections does not match.");
    }

    [Test]
    public void GetSectionCorrectly()
    {
        // Arrange
        List<SectionToggleModel> testSections = CreateTestSections(2);
        sectionSelectorComponent.SetSections(testSections);

        // Act
        ISectionToggle existingSection1 = sectionSelectorComponent.GetSection(0);
        ISectionToggle existingSection2 = sectionSelectorComponent.GetSection(1);

        // Assert
        Assert.AreEqual(testSections[0].title, existingSection1.GetInfo().title, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].title, existingSection2.GetInfo().title, "The item 2 gotten does not match.");
    }

    [Test]
    public void GetAllSectionsCorrectly()
    {
        // Arrange
        List<SectionToggleModel> testSections = CreateTestSections(2);
        sectionSelectorComponent.SetSections(testSections);

        // Act
        List<ISectionToggle> allExistingItems = sectionSelectorComponent.GetAllSections();

        // Assert
        Assert.AreEqual(testSections[0].title, allExistingItems[0].GetInfo().title, "The section 1 gotten does not match.");
        Assert.AreEqual(testSections[1].title, allExistingItems[1].GetInfo().title, "The section 2 gotten does not match.");
        Assert.AreEqual(testSections.Count, allExistingItems.Count, "The number of sections gotten do not match.");
    }

    [UnityTest]
    public IEnumerator RemoveAllInstantiatedSectionsCorrectly()
    {
        // Arrange
        List<SectionToggleModel> testSections = CreateTestSections(2);
        sectionSelectorComponent.SetSections(testSections);

        // Act
        sectionSelectorComponent.RemoveAllIntantiatedSections();
        yield return null;

        // Assert
        Assert.AreEqual(1, sectionSelectorComponent.transform.childCount, "The number of sections does not match.");
    }

    private SectionSelectorComponentModel CreateTestModel(int numberOfSections)
    {
        SectionSelectorComponentModel testModel = new SectionSelectorComponentModel
        {
            sections = CreateTestSections(numberOfSections)
        };

        return testModel;
    }

    private List<SectionToggleModel> CreateTestSections(int numberOfSections)
    {
        List<SectionToggleModel> sections = new List<SectionToggleModel>();

        for (int i = 0; i < numberOfSections; i++)
        {
            sections.Add(new SectionToggleModel
            {
                icon = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero),
                title = $"Test{i + 1}",
                onSelectEvent = new Toggle.ToggleEvent()
            });
        }

        return sections;
    }
}