using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

public class ExploreV2FeatureTests
{
    private ExploreV2Feature exploreV2Feature;
    private IExploreV2MenuComponentController exploreV2MenuComponentController;

    [SetUp]
    public void SetUp()
    {
        exploreV2MenuComponentController = Substitute.For<IExploreV2MenuComponentController>();
        exploreV2Feature = Substitute.ForPartsOf<ExploreV2Feature>();
        exploreV2Feature.Configure().CreateController().Returns(info => exploreV2MenuComponentController);
    }

    [TearDown]
    public void TearDown() { exploreV2Feature.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Act
        exploreV2Feature.Enable();

        // Assert
        exploreV2MenuComponentController.Received().Initialize();
    }
}