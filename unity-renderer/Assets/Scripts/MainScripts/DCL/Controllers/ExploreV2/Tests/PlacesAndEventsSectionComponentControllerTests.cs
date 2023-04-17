using DCL;
using ExploreV2Analytics;
using NSubstitute;
using NUnit.Framework;

public class PlacesAndEventsSectionComponentControllerTests
{
    private PlacesAndEventsSectionComponentController placesAndEventsSectionComponentController;
    private IPlacesAndEventsSectionComponentView placesAndEventsSectionComponentView;
    private IExploreV2Analytics exploreV2Analytics;

    [SetUp]
    public void SetUp()
    {
        placesAndEventsSectionComponentView = Substitute.For<IPlacesAndEventsSectionComponentView>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        placesAndEventsSectionComponentController = new PlacesAndEventsSectionComponentController(placesAndEventsSectionComponentView, exploreV2Analytics, DataStore.i, Substitute.For<IUserProfileBridge>());
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
        Assert.IsNotNull(placesAndEventsSectionComponentController.highlightsSubSectionComponentController);
    }

    [Test]
    public void RequestExploreV2ClosingCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        placesAndEventsSectionComponentController.OnCloseExploreV2 += (fromShortcut) => exploreClosed = true;

        // Act
        placesAndEventsSectionComponentController.RequestExploreV2Closing();

        // Assert
        Assert.IsTrue(exploreClosed);
    }

    [Test]
    public void GoToEventsSubSectionCorrectly()
    {
        // Act
        placesAndEventsSectionComponentController.GoToEventsSubSection();

        // Assert
        placesAndEventsSectionComponentView.Received().GoToSubsection(PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaisePlacesAndEventsVisibleChangedCorrectly(bool isVisible)
    {
        // Act
        placesAndEventsSectionComponentController.PlacesAndEventsVisibleChanged(isVisible, !isVisible);

        // Arrange
        placesAndEventsSectionComponentView.Received().SetActive(isVisible);
    }
}
