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
        loadHelper.OnFetchInfoSuccess += info => Assert.Fail();
        loadHelper.OnFetchInfoFail += Assert.Pass;
        loadHelper.FetchNFTInfo("testAddress", "testId");
    }

    [Test]
    public void DisposeCorrectly()
    {
        //Arrange
        loadHelper.FetchNFTInfo("testAddress", "testId");

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