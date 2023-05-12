using NUnit.Framework;

public class SearchBarComponentViewTests
{
    private SearchBarComponentView searchBarComponent;

    [SetUp]
    public void SetUp()
    {
        searchBarComponent = BaseComponentView.CreateUIComponentFromAssetDatabase<SearchBarComponentView>("SearchBar");
        searchBarComponent.model.idleTimeToTriggerSearch = 0;
    }

    [TearDown]
    public void TearDown()
    {
        searchBarComponent.Dispose();
    }

    [Test]
    public void ConfigureSearchBarCorrectly()
    {
        // Arrange
        SearchBarComponentModel testModel = new SearchBarComponentModel
        {
            idleTimeToTriggerSearch = 1,
            placeHolderText = "Test"
        };

        // Act
        searchBarComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, searchBarComponent.model, "The model does not match after configuring the search bar.");
    }

    [Test]
    public void SetSearchBarPlaceHolderTextCorrectly()
    {
        // Arrange
        string testText = "Test";

        // Act
        searchBarComponent.SetPlaceHolderText(testText);

        // Assert
        Assert.AreEqual(testText, searchBarComponent.model.placeHolderText, "The place holder text does not match in the model.");
        Assert.AreEqual(testText, searchBarComponent.placeHolderText.text, "The search bar place holder text does not match.");
    }

    [Test]
    public void SubmitSearchCorrectly()
    {
        // Arrange
        string testText = "Test";
        searchBarComponent.inputField.text = "";
        searchBarComponent.clearSearchButton.gameObject.SetActive(false);
        searchBarComponent.searchSpinner.SetActive(true);

        string searchedString = "";
        searchBarComponent.OnSearchText += (value) =>
        {
            searchedString = value;
        };

        // Act
        searchBarComponent.SubmitSearch(testText);

        // Assert
        Assert.AreEqual(testText, searchBarComponent.inputField.text, "The search bar text does not match.");
        Assert.AreEqual(testText, searchedString, "The searched string does not match.");
        Assert.IsTrue(searchBarComponent.clearSearchButton.gameObject.activeSelf);
        Assert.IsFalse(searchBarComponent.searchSpinner.activeSelf);
    }

    [Test]
    public void ClearSearchCorrectly()
    {
        // Arrange
        searchBarComponent.inputField.text = "Test";
        searchBarComponent.clearSearchButton.gameObject.SetActive(true);
        searchBarComponent.searchSpinner.SetActive(true);

        // Act
        searchBarComponent.ClearSearch();

        // Assert
        Assert.IsEmpty(searchBarComponent.inputField.text, "The search bar text is not empty.");
        Assert.IsFalse(searchBarComponent.clearSearchButton.gameObject.activeSelf);
        Assert.IsFalse(searchBarComponent.searchSpinner.activeSelf);
    }

    [Test]
    public void SetIdleSearchTimeCorrectly()
    {
        // Arrange
        float testTime = 10f;

        // Act
        searchBarComponent.SetIdleSearchTime(testTime);

        // Assert
        Assert.AreEqual(testTime, searchBarComponent.model.idleTimeToTriggerSearch, "The idle search time does not match in the model.");
    }

    [Test]
    public void SetTypingModeCorrectly()
    {
        // Arrange
        searchBarComponent.clearSearchButton.gameObject.SetActive(true);
        searchBarComponent.searchSpinner.SetActive(false);

        // Act
        searchBarComponent.SetTypingMode();

        // Assert
        Assert.IsFalse(searchBarComponent.clearSearchButton.gameObject.activeSelf);
        Assert.IsTrue(searchBarComponent.searchSpinner.activeSelf);
    }

    [Test]
    public void SetSearchModeCorrectly()
    {
        // Arrange
        searchBarComponent.clearSearchButton.gameObject.SetActive(false);
        searchBarComponent.searchSpinner.SetActive(true);

        // Act
        searchBarComponent.SetSearchMode();

        // Assert
        Assert.IsTrue(searchBarComponent.clearSearchButton.gameObject.activeSelf);
        Assert.IsFalse(searchBarComponent.searchSpinner.activeSelf);
    }

    [Test]
    public void SetClearModeCorrectly()
    {
        // Arrange
        searchBarComponent.clearSearchButton.gameObject.SetActive(true);
        searchBarComponent.searchSpinner.SetActive(true);

        // Act
        searchBarComponent.SetClearMode();

        // Assert
        Assert.IsFalse(searchBarComponent.clearSearchButton.gameObject.activeSelf);
        Assert.IsFalse(searchBarComponent.searchSpinner.activeSelf);
    }
}
