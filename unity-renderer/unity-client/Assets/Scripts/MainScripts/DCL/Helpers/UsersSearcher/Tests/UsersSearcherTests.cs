using NSubstitute;
using NUnit.Framework;

public class UsersSearcherTests
{
    [Test]
    public void SearchPromiseShouldResolveLastSearchOnly()
    {
        var bridge = Substitute.For<IUsersSearchBridge>();
        var searcher = new UsersSearcher(bridge);

        bool firstSearchCalled = false;
        bool secondSearchCalled = false;

        searcher.SearchUser("TheRealPravus", 1)
                .Then(profiles =>
                {
                    firstSearchCalled = true;
                });
        searcher.SearchUser("TheRealPravus2", 1)
                .Then(profiles =>
                {
                    secondSearchCalled = true;
                });

        bridge.OnSearchResult += Raise.Event<OnSearchResultDelegate>("TheRealPravus2", null);

        Assert.IsFalse(firstSearchCalled);
        Assert.IsTrue(secondSearchCalled);
        
        searcher.Dispose();
    }

    [Test]
    public void DifferentSearcherInstancesResultShouldNotCollide()
    {
        const string VAL1 = "first";
        const string VAL2 = "second";
        
        var bridge = Substitute.For<IUsersSearchBridge>();
        var searcher1 = new UsersSearcher(bridge);
        var searcher2 = new UsersSearcher(bridge);

        string firstSearchValue = string.Empty;
        string secondSearchValue= string.Empty;

        searcher1.SearchUser("TheRealPravus", 1)
                 .Then(profiles =>
                 {
                     firstSearchValue = profiles[0].name;
                 });
        searcher2.SearchUser("TheRealPravus2", 1)
                 .Then(profiles =>
                 {
                     secondSearchValue = profiles[0].name;
                 });

        bridge.OnSearchResult += Raise.Event<OnSearchResultDelegate>("TheRealPravus", new [] { new UserProfileModel() { name = VAL1 } });
        bridge.OnSearchResult += Raise.Event<OnSearchResultDelegate>("TheRealPravus2", new [] { new UserProfileModel() { name = VAL2 } });

        Assert.AreNotEqual(firstSearchValue, secondSearchValue);
        Assert.AreEqual(VAL1, firstSearchValue);
        Assert.AreEqual(VAL2, secondSearchValue);
        
        searcher1.Dispose();
        searcher2.Dispose();
    }
}