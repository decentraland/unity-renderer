using NUnit.Framework;
using NUnit.Framework.Internal;

public class NFTInfoLoadHelperShould
{
    private NFTInfoRetriever retriever;

    [SetUp]
    protected void SetUp()
    {
        retriever = new NFTInfoRetriever();
    }

    [Test]
    public void CallFailWhenFetchFails()
    {
        //Act - Assert
        retriever.OnFetchInfoSuccess += info => Assert.Fail();
        retriever.OnFetchInfoFail += Assert.Pass;
        retriever.FetchNFTInfo("ethereum", "testAddress", "testId");
    }

    [Test]
    public void DisposeCorrectly()
    {
        //Arrange
        retriever.FetchNFTInfo("ethereum", "testAddress", "testId");

        //Act
        retriever.Dispose();

        //Assert
        Assert.IsNull(retriever.fetchCoroutine);
    }

    [TearDown]
    protected void TearDown()
    {
        retriever.Dispose();
    }
}