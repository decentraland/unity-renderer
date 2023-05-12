using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

public class SectionSelectorComponentViewTests
{
    private SectionSelectorComponentView sectionSelectorComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        sectionSelectorComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<SectionSelectorComponentView>("SectionSelector");
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        sectionSelectorComponent.Dispose();
        GameObject.Destroy(sectionSelectorComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
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
        Assert.AreEqual(testSections[0].selectedIcon, existingSection1.GetInfo().selectedIcon, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].selectedIcon, existingSection2.GetInfo().selectedIcon, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].selectedTitle, existingSection1.GetInfo().selectedTitle, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].selectedTitle, existingSection2.GetInfo().selectedTitle, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].selectedTextColor, existingSection1.GetInfo().selectedTextColor, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].selectedTextColor, existingSection2.GetInfo().selectedTextColor, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].selectedImageColor, existingSection1.GetInfo().selectedImageColor, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].selectedImageColor, existingSection2.GetInfo().selectedImageColor, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].unselectedIcon, existingSection1.GetInfo().unselectedIcon, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].unselectedIcon, existingSection2.GetInfo().unselectedIcon, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].unselectedTitle, existingSection1.GetInfo().unselectedTitle, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].unselectedTitle, existingSection2.GetInfo().unselectedTitle, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].backgroundTransitionColorsForSelected, existingSection1.GetInfo().backgroundTransitionColorsForSelected, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].backgroundTransitionColorsForSelected, existingSection2.GetInfo().backgroundTransitionColorsForSelected, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].unselectedTextColor, existingSection1.GetInfo().unselectedTextColor, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].unselectedTextColor, existingSection2.GetInfo().unselectedTextColor, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].unselectedImageColor, existingSection1.GetInfo().unselectedImageColor, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].unselectedImageColor, existingSection2.GetInfo().unselectedImageColor, "The item 2 gotten does not match.");
        Assert.AreEqual(testSections[0].backgroundTransitionColorsForUnselected, existingSection1.GetInfo().backgroundTransitionColorsForUnselected, "The item 1 gotten does not match.");
        Assert.AreEqual(testSections[1].backgroundTransitionColorsForUnselected, existingSection2.GetInfo().backgroundTransitionColorsForUnselected, "The item 2 gotten does not match.");
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
        Assert.AreEqual(testSections[0].selectedTitle, allExistingItems[0].GetInfo().selectedTitle, "The section 1 gotten does not match.");
        Assert.AreEqual(testSections[1].selectedTitle, allExistingItems[1].GetInfo().selectedTitle, "The section 2 gotten does not match.");
        Assert.AreEqual(testSections.Count, allExistingItems.Count, "The number of sections gotten do not match.");
    }

    [Test]
    public void CreateItemCorrectly()
    {
        // Arrange
        SectionToggleModel testSection = new SectionToggleModel
        {
            selectedIcon = testSprite,
            selectedTitle = "Test Title",
        };
        string testName = "TestName";

        // Act
        sectionSelectorComponent.CreateSection(testSection, testName);

        // Assert
        ISectionToggle newSection = sectionSelectorComponent.instantiatedSections.FirstOrDefault(x => x.GetInfo().selectedTitle == testSection.selectedTitle);
        Assert.IsNotNull(newSection, "The item does not exist in the instantiatedItems list.");
        Assert.IsTrue(newSection.GetInfo().selectedTitle == testSection.selectedTitle, "The item title does not match.");
        Assert.IsTrue(newSection.GetInfo().selectedIcon == testSection.selectedIcon, "The item icon does not match.");
        Assert.AreEqual(testName, ((SectionToggle)newSection).name, "The item game object name does not match.");
    }

    [UnityTest]
    public IEnumerator RemoveAllInstantiatedSectionsCorrectly()
    {
        // Arrange
        List<SectionToggleModel> testSections = CreateTestSections(2);
        sectionSelectorComponent.SetSections(testSections);

        // Act
        sectionSelectorComponent.RemoveAllInstantiatedSections();
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
                selectedIcon = testSprite,
                selectedTitle = $"Test{i + 1}"
            });
        }

        return sections;
    }
}
