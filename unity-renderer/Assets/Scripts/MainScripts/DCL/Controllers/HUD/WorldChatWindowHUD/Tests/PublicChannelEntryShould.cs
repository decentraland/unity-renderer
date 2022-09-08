using DCL.Chat.HUD;
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
        view.Configure(new PublicChatEntryModel("nearby", "nearby", 0, true, 4, false));
        view.nameLabel.text = "#nearby";
        Assert.IsFalse(view.muteNotificationsToggle.isOn);
        Assert.AreEqual("4 members", view.memberCountLabel.text);
    }

    [Test]
    public void TriggerOpenChat()
    {
        var called = false;
        view.OnOpenChat += entry => called = true;
        
        view.openChatButton.onClick.Invoke();
        
        Assert.IsTrue(called);
    }

    [Test]
    public void ConfigureAsMuted()
    {
        view.Configure(new PublicChatEntryModel("nearby", "nearby", 0, true, 0, true));
        Assert.IsTrue(view.muteNotificationsToggle.isOn);
    }
}