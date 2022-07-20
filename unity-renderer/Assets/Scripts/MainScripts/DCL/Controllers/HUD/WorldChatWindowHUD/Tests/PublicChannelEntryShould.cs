using NSubstitute;
using NUnit.Framework;

public class PublicChannelEntryShould
{
    private PublicChatEntry view;

    [SetUp]
    public void SetUp()
    {
        view = PublicChatEntry.Create();
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
        view.Configure(new PublicChatEntry.PublicChatEntryModel("nearby", "~nearby", 0, true, 0));
        view.nameLabel.text = "~nearby";
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