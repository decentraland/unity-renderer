using NUnit.Framework;
using NUnit.Framework.Internal;

public class NFTInfoLoadHelperShould
{
    private NFTInfoLoadHelper loadHelper;

    [SetUp]
    protected void SetUp()
    {
        loadHelper = new NFTInfoLoadHelper();
    }

    [Test]
    public void CallFailWhenFetchFails()
    {
        //Act - Assert
        loadHelper.FetchNFTInfo("testAddress", "testId", info => Assert.Fail(), Assert.Pass);
    }

    [Test]
    public void DisposeCorrectly()
    {
        //Arrange
        loadHelper.FetchNFTInfo("testAddress", "testId", null, null);

        //Act
        loadHelper.Dispose();

        //Assert
        Assert.IsNull(loadHelper.fetchCoroutine);
    }

    [TearDown]
    protected void TearDown()
    {
        loadHelper.Dispose();
    }
}