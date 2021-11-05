using NSubstitute;
using NUnit.Framework;

public class PlacesAndEventsSectionComponentControllerTests
{
    private PlacesAndEventsSectionComponentController placesAndEventsSectionComponentController;
    private IPlacesAndEventsSectionComponentView placesAndEventsSectionComponentView;

    [SetUp]
    public void SetUp()
    {
        placesAndEventsSectionComponentView = Substitute.For<IPlacesAndEventsSectionComponentView>();
        placesAndEventsSectionComponentController = new PlacesAndEventsSectionComponentController(placesAndEventsSectionComponentView);
    }

    [TearDown]
    public void TearDown() { placesAndEventsSectionComponentController.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(placesAndEventsSectionComponentView, placesAndEventsSectionComponentController.view);
        Assert.IsNotNull(placesAndEventsSectionComponentController.placesSubSectionComponentController);
        Assert.IsNotNull(placesAndEventsSectionComponentController.eventsSubSectionComponentController);
    }

    [Test]
    public void RequestExploreV2ClosingCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        placesAndEventsSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;

        // Act
        placesAndEventsSectionComponentController.RequestExploreV2Closing();

        // Assert
        Assert.IsTrue(exploreClosed);
    }
}