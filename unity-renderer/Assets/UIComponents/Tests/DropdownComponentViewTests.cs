using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TestTools;

public class DropdownComponentViewTests
{
    private DropdownComponentView dropdownComponent;

    [SetUp]
    public void SetUp()
    {
        dropdownComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<DropdownComponentView>("Dropdown_MultiSelect");
    }

    [TearDown]
    public void TearDown()
    {
        dropdownComponent.Dispose();
    }

    [Test]
    public void ConfigureDropdownCorrectly()
    {
        // Arrange
        DropdownComponentModel testModel = new DropdownComponentModel
        {
            isMultiselect = true,
            title = "Test",
            options = new List<ToggleComponentModel>
            {
                new ToggleComponentModel
                {
                    id = "1",
                    text = "Test1",
                    isOn = false
                },
                new ToggleComponentModel
                {
                    id = "2",
                    text = "Test2",
                    isOn = false
                },
                new ToggleComponentModel
                {
                    id = "3",
                    text = "Test3",
                    isOn = false
                }
            }
        };

        // Act
        dropdownComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, dropdownComponent.model, "The model does not match after configuring the dropdown.");
    }

    [Test]
    public void OpenDropdownCorrectly()
    {
        // Arrange
        dropdownComponent.Close();

        // Act
        dropdownComponent.Open();

        // Assert
        Assert.IsTrue(dropdownComponent.optionsPanel.activeSelf);
        Assert.IsTrue(dropdownComponent.isOpen);
    }

    [Test]
    public void CloseDropdownCorrectly()
    {
        // Arrange
        dropdownComponent.Open();

        // Act
        dropdownComponent.Close();

        // Assert
        Assert.IsFalse(dropdownComponent.optionsPanel.activeSelf);
        Assert.IsFalse(dropdownComponent.isOpen);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ToggleOptionsListCorrectly(bool initiallyOpen)
    {
        // Arrange
        if (initiallyOpen)
            dropdownComponent.Open();
        else
            dropdownComponent.Close();

        // Act
        dropdownComponent.ToggleOptionsList();

        // Assert
        if (initiallyOpen)
        {
            Assert.IsFalse(dropdownComponent.optionsPanel.activeSelf);
            Assert.IsFalse(dropdownComponent.isOpen);
        }
        else
        {
            Assert.IsTrue(dropdownComponent.optionsPanel.activeSelf);
            Assert.IsTrue(dropdownComponent.isOpen);
        }
    }

    [Test]
    public void SetDropdownTitleCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        dropdownComponent.SetTitle(testText);

        // Assert
        Assert.AreEqual(testText, dropdownComponent.model.title, "The title does not match in the model.");
        Assert.AreEqual(testText, dropdownComponent.title.text, "The title text does not match.");
    }

    [Test]
    public void SetOptionsCorrectly()
    {
        // Arrange
        List<ToggleComponentModel> testOptions = CreateTestOptions(3);

        // Act
        dropdownComponent.SetOptions(testOptions);

        // Assert
        Assert.AreEqual(testOptions, dropdownComponent.model.options, "The options list does not match in the model.");
        Assert.AreEqual(testOptions, dropdownComponent.originalOptions, "The options list does not match in the original list.");
        Assert.AreEqual(testOptions.Count + 1, dropdownComponent.availableOptions.transform.childCount, "The number of instantiated options does not match.");
    }

    [Test]
    public void FilterOptionsCorrectly()
    {
        // Arrange
        string testText = "2";
        List<ToggleComponentModel> testOptions = CreateTestOptions(3);
        dropdownComponent.SetOptions(testOptions);

        // Act
        dropdownComponent.FilterOptions(testText);

        // Assert
        Assert.AreEqual(1, dropdownComponent.availableOptions.transform.childCount, "The number of instantiated options does not match.");
        Assert.AreEqual(testOptions, dropdownComponent.originalOptions, "The options list does not match in the original list.");
        Assert.AreEqual(testOptions[1], dropdownComponent.model.options[0]);
    }

    [Test]
    public void GetOptionCorrectly()
    {
        // Arrange
        List<ToggleComponentModel> testOptions = CreateTestOptions(2);
        dropdownComponent.SetOptions(testOptions);

        // Act
        IToggleComponentView existingOption1 = dropdownComponent.GetOption(0);
        IToggleComponentView existingOption2 = dropdownComponent.GetOption(1);

        // Assert
        Assert.AreEqual(testOptions[0].id, existingOption1.id, "The option 1 gotten does not match.");
        Assert.AreEqual(testOptions[1].id, existingOption2.id, "The option 2 gotten does not match.");
    }

    [Test]
    public void GetAllOptionsCorrectly()
    {
        // Arrange
        List<ToggleComponentModel> testOptions = CreateTestOptions(2);
        dropdownComponent.SetOptions(testOptions);

        // Act
        List<IToggleComponentView> allExistingOptions = dropdownComponent.GetAllOptions();

        // Assert
        Assert.AreEqual(testOptions[0].id, allExistingOptions[0].id, "The option 1 gotten does not match.");
        Assert.AreEqual(testOptions[1].id, allExistingOptions[1].id, "The option 2 gotten does not match.");
        Assert.AreEqual(testOptions.Count, allExistingOptions.Count, "The number of options gotten do not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetSelectAllCorrectly(bool isSelected)
    {
        // Arrange
        List<ToggleComponentModel> testOptions = CreateTestOptions(2, !isSelected);
        dropdownComponent.SetOptions(testOptions);
        List<IToggleComponentView> allExistingOptions = dropdownComponent.GetAllOptions();

        // Act
        dropdownComponent.SetSelectAll(isSelected);

        // Assert
        Assert.AreEqual(isSelected, allExistingOptions[0].isOn, "The option 1 gotten isOn property does not match.");
        Assert.AreEqual(isSelected, allExistingOptions[1].isOn, "The option 2 gotten isOn property does not match.");
    }

    [Test]
    public void SetSetSearchPlaceHolderTextCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        dropdownComponent.SetSearchPlaceHolderText(testText);

        // Assert
        Assert.AreEqual(testText, dropdownComponent.model.searchPlaceHolderText, "The search place holder does not match in the model.");
        Assert.AreEqual(testText, dropdownComponent.searchBar.placeHolderText.text, "The search place holder text does not match.");
    }

    [Test]
    public void SetSearchNotFoundTextCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        dropdownComponent.SetSearchNotFoundText(testText);

        // Assert
        Assert.AreEqual(testText, dropdownComponent.model.searchNotFoundText, "The searchNotFoundText does not match in the model.");
        Assert.AreEqual(testText, dropdownComponent.searchNotFoundMessage.text, "The searchNotFoundMessage text does not match.");
    }

    [Test]
    public void SetEmptyContentTextCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        dropdownComponent.SetEmptyContentText(testText);

        // Assert
        Assert.AreEqual(testText, dropdownComponent.model.emptyContentText, "The emptyContentText does not match in the model.");
        Assert.AreEqual(testText, dropdownComponent.emptyContentMessage.text, "The emptyContentMessage text does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetLoadingActiveCorrectly(bool isActive)
    {
        // Arrange
        dropdownComponent.loadingPanel.SetActive(!isActive);

        // Act
        dropdownComponent.SetLoadingActive(isActive);

        // Assert
        Assert.AreEqual(isActive, dropdownComponent.loadingPanel.activeSelf, "The loading panel active property does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetSelectAllOptionActiveCorrectly(bool isActive)
    {
        // Arrange
        List<ToggleComponentModel> testOptions = CreateTestOptions(1);
        dropdownComponent.SetOptions(testOptions);
        dropdownComponent.model.showSelectAllOption = !isActive;
        dropdownComponent.selectAllOptionComponent.gameObject.SetActive(!isActive);

        // Act
        dropdownComponent.SetSelectAllOptionActive(isActive);

        // Assert
        Assert.AreEqual(isActive, dropdownComponent.model.showSelectAllOption, "The showSelectAllOption property does not match in the model.");
        Assert.AreEqual(isActive, dropdownComponent.selectAllOptionComponent.gameObject.activeSelf, "The selectAllOptionComponent active property does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetOptionsPanelHeightAsDynamicCorrectly(bool isDynamic)
    {
        // Arrange
        float testHeight = 100f;
        dropdownComponent.model.isOptionsPanelHeightDynamic = !isDynamic;
        dropdownComponent.model.maxValueForDynamicHeight = 0f;

        // Act
        dropdownComponent.SetOptionsPanelHeightAsDynamic(isDynamic, testHeight);

        // Assert
        Assert.AreEqual(isDynamic, dropdownComponent.model.isOptionsPanelHeightDynamic, "The isOptionsPanelHeightDynamic property does not match in the model.");
        Assert.AreEqual(testHeight, dropdownComponent.model.maxValueForDynamicHeight, "The maxValueForDynamicHeight property does not match in the model.");
    }

    [Test]
    public void CreateSelectAllOptionCorrectly()
    {
        // Act
        dropdownComponent.CreateSelectAllOption();

        // Assert
        ToggleComponentView newOption = dropdownComponent.availableOptions
            .GetItems()
            .Select(x => x as ToggleComponentView)
            .FirstOrDefault(x => x.model.id == DropdownComponentView.SELECT_ALL_OPTION_ID);

        Assert.IsNotNull(newOption, "The Select All option does not exist in the availableOptions list.");
        Assert.IsTrue(newOption.model.id == DropdownComponentView.SELECT_ALL_OPTION_ID, "The option id does not match.");
        Assert.IsTrue(newOption.model.isOn == false, "The Select All option isOn property does not match.");
        Assert.IsTrue(newOption.model.text == DropdownComponentView.SELECT_ALL_OPTION_TEXT, "The Select All option text does not match.");
        Assert.AreEqual($"Option_{DropdownComponentView.SELECT_ALL_OPTION_ID}", newOption.name, "The Select All option game object name does not match.");
    }

    [Test]
    public void CreateOptionCorrectly()
    {
        // Arrange
        ToggleComponentModel testOption = new ToggleComponentModel
        {
            id = "TestId",
            isOn = true,
            text = "Test"
        };
        string testName = "TestName";

        // Act
        dropdownComponent.CreateOption(testOption, testName);

        // Assert
        ToggleComponentView newOption = dropdownComponent.availableOptions
            .GetItems()
            .Select(x => x as ToggleComponentView)
            .FirstOrDefault(x => x.model.id == testOption.id);

        Assert.IsNotNull(newOption, "The option does not exist in the availableOptions list.");
        Assert.IsTrue(newOption.model.id == testOption.id, "The option id does not match.");
        Assert.IsTrue(newOption.model.isOn == testOption.isOn, "The option isOn property does not match.");
        Assert.IsTrue(newOption.model.text == testOption.text, "The option text does not match.");
        Assert.AreEqual(testName, newOption.name, "The option game object name does not match.");
    }

    [Test]
    [TestCase("1")]
    [TestCase("2")]
    public void SelectOptionCorrectly(string selectedId)
    {
        // Arrange
        List<ToggleComponentModel> testOptions = CreateTestOptions(2);
        dropdownComponent.SetOptions(testOptions);
        dropdownComponent.isMultiselect = true;

        bool isOptionSelected = false;
        string selectedIdConfirmation = "";
        dropdownComponent.OnOptionSelectionChanged += (isOn, id, name) =>
        {
            isOptionSelected = true;
            selectedIdConfirmation = id;
        };

        // Act
        dropdownComponent.OnOptionSelected(true, selectedId, "TestName");

        // Assert
        Assert.IsTrue(isOptionSelected);
        Assert.AreEqual(selectedId, selectedIdConfirmation);
    }

    [UnityTest]
    public IEnumerator RemoveAllInstantiatedOptionsCorrectly()
    {
        // Arrange
        List<ToggleComponentModel> testOptions = CreateTestOptions(2);
        dropdownComponent.SetOptions(testOptions);

        // Act
        dropdownComponent.RemoveAllInstantiatedOptions();
        yield return null;

        // Assert
        Assert.AreEqual(0, dropdownComponent.availableOptions.transform.childCount, "The number of option does not match.");
    }

    private List<ToggleComponentModel> CreateTestOptions(int numberOfOptions, bool isOn = false)
    {
        List<ToggleComponentModel> options = new List<ToggleComponentModel>();

        for (int i = 0; i < numberOfOptions; i++)
        {
            options.Add(new ToggleComponentModel
            {
                id = $"{i + 1}",
                isOn = isOn,
                text = $"Test{i + 1}"
            });
        }

        return options;
    }
}
