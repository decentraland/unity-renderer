using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

public class ExploreV2FeatureTests
{
    private ExploreV2Feature exploreV2Feature;
    private IExploreV2MenuComponentView exploreV2MenuView;

    [SetUp]
    public void SetUp()
    {
        exploreV2MenuView = Substitute.For<IExploreV2MenuComponentView>();
        exploreV2Feature = Substitute.ForPartsOf<ExploreV2Feature>();
        exploreV2Feature.Configure().CreateView().Returns(info => exploreV2MenuView);
        exploreV2Feature.Initialize();
    }

    [TearDown]
    public void TearDown() { exploreV2Feature.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(exploreV2MenuView, exploreV2Feature.view);
        exploreV2MenuView.Received().SetActive(false);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVisibilityCorrectly(bool isVisible)
    {
        // Act
        exploreV2Feature.SetVisibility(isVisible);

        //Assert
        exploreV2MenuView.Received().SetActive(isVisible);
    }

    [Test]
    public void ClickOnCloseButtonCorrectly()
    {
        // Arrange
        bool clicked = false;
        exploreV2Feature.OnClose += () => clicked = true;

        // Act
        exploreV2Feature.View_OnCloseButtonPressed();

        // Assert
        Assert.IsTrue(clicked);
    }
}