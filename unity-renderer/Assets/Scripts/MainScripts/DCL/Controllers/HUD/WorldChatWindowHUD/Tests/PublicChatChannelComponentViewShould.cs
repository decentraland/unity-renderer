using NUnit.Framework;

public class PublicChatChannelComponentViewShould
{
    private PublicChatWindowComponentView view;
    
    [SetUp]
    public void SetUp()
    {
        view = PublicChatWindowComponentView.Create();
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
    }

    [Test]
    public void Show()
    {
        view.Show();
        
        Assert.IsTrue(view.gameObject.activeSelf);
    }
    
    [Test]
    public void Hide()
    {
        view.Hide();
        
        Assert.IsFalse(view.gameObject.activeSelf);
    }

    [Test]
    public void Configure()
    {
        view.Configure(new PublicChatModel("nearby", "nearby", "any description", true, 0, false));
        
        Assert.AreEqual("~nearby", view.nameLabel.text);
        Assert.IsFalse(view.muteToggle.isOn);
    }
    
    [Test]
    public void ConfigureAsMuted()
    {
        view.Configure(new PublicChatModel("nearby", "nearby", "any description", true, 0, true));
        
        Assert.AreEqual("~nearby", view.nameLabel.text);
        Assert.IsTrue(view.muteToggle.isOn);
    }

    [Test]
    public void TriggerClose()
    {
        var called = false;
        view.OnClose += () => called = true;
        
        view.closeButton.onClick.Invoke();
        
        Assert.IsTrue(called);
    }
    
    [Test]
    public void TriggerBack()
    {
        var called = false;
        view.OnBack += () => called = true;
        
        view.backButton.onClick.Invoke();
        
        Assert.IsTrue(called);
    }

    [Test]
    public void TriggerFocusWhenWindowIsClicked()
    {
        var clicked = false;
        view.OnClickOverWindow += () => clicked = true;
        
        view.OnPointerDown(null);
        
        Assert.IsTrue(clicked);
    }
    
    [Test]
    public void TriggerFocusWhenWindowIsHovered()
    {
        var focused = false;
        view.OnFocused += f => focused = f;
        
        view.OnPointerEnter(null);
        
        Assert.IsTrue(focused);
    }
    
    [Test]
    public void TriggerUnfocusWhenPointerExits()
    {
        var focused = true;
        view.OnFocused += f => focused = f;
        
        view.OnPointerExit(null);
        
        Assert.IsFalse(focused);
    }
}