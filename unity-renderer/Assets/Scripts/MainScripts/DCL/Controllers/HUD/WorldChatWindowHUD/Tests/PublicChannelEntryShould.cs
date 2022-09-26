using NSubstitute;
using NUnit.Framework;

public class PublicChannelEntryShould
{
    private PublicChannelEntry view;

    [SetUp]
    public void SetUp()
    {
        view = PublicChannelEntry.Create();
        view.Initialize(Substitute.For<IChatController>());
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
    }

    [Test]
    public void Configure()
    {
        view.Configure(new PublicChannelEntry.PublicChannelEntryModel("nearby", "nearby"));
        view.nameLabel.text = "#nearby";
    }

    [Test]
    public void TriggerOpenChat()
    {
        var called = false;
        view.OnOpenChat += entry => called = true;
        
        view.openChatButton.onClick.Invoke();
        
        Assert.IsTrue(called);
    }
}