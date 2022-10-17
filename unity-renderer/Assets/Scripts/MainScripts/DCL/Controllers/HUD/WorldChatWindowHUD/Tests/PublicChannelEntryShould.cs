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
    [TestCase(true)]
    [TestCase(false)]
    public void Configure(bool showOnlyOnlineMembers)
    {
        view.Configure(new PublicChatEntryModel("nearby", "nearby", true, 4, showOnlyOnlineMembers, false));
        view.nameLabel.text = "#nearby";
        Assert.IsFalse(view.muteNotificationsToggle.isOn);
        Assert.AreEqual($"4 members {(showOnlyOnlineMembers ? "online" : "joined")}", view.memberCountLabel.text);
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
        view.Configure(new PublicChatEntryModel("nearby", "nearby", true, 0, false, true));
        Assert.IsTrue(view.muteNotificationsToggle.isOn);
    }
}