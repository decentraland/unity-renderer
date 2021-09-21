using NUnit.Framework;
using NUnit.Framework.Internal;

public class NFTInfoFectcherShould
{
    private NFTInfoFetcher fetcher;

    [SetUp]
    protected void SetUp() {  fetcher = new NFTInfoFetcher(); }

    [Test]
    public void NFTInfoFetcherFail()
    {
        //Act - Assert
        fetcher.FetchNFTImage("testAddress", "testId", info =>  Assert.Fail(), Assert.Pass );
    }

    [Test]
    public void NFTInfoFetcherDipose()
    {
        //Arrange
        fetcher.FetchNFTImage("testAddress", "testId", null, null );

        //Act
        fetcher.Dispose();

        //Assert
        Assert.IsNull(fetcher.fetchCoroutine);
    }

    [TearDown]
    protected void TearDown() { fetcher.Dispose(); }
}