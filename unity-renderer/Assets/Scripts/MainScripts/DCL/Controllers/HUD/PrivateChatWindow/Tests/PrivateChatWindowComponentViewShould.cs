using System.Collections;
using NUnit.Framework;
using SocialBar.UserThumbnail;
using UnityEngine;
using UnityEngine.TestTools;

public class UserThumbnailComponentViewMock : UserThumbnailComponentView
{
    public UserThumbnailComponentModel ConfiguredWith { get; private set; }

    public override void Configure(UserThumbnailComponentModel model) => ConfiguredWith = model;
}

public class PrivateChatWindowComponentViewShould
{
    private PrivateChatWindowComponentView view;
    private UserThumbnailComponentViewMock userThumbnail;

    [SetUp]
    public void SetUp()
    {
        view = PrivateChatWindowComponentView.Create();
        userThumbnail = new GameObject("userThumbnail").AddComponent<UserThumbnailComponentViewMock>();
        view.userThumbnail = userThumbnail;
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
        Object.Destroy(userThumbnail.gameObject);
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

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void Configure(bool online, bool blocked)
    {
        var profile = ScriptableObject.CreateInstance<UserProfile>();
        profile.UpdateData(new UserProfileModel{userId = "userId", name = "name", snapshots = new UserProfileModel.Snapshots{face256 = "someurl"}});
        
        view.Setup(profile, online, blocked);
        
        Assert.AreEqual("someurl", userThumbnail.ConfiguredWith.faceUrl);
        Assert.AreEqual(blocked, userThumbnail.ConfiguredWith.isBlocked);
        Assert.AreEqual(online, userThumbnail.ConfiguredWith.isOnline);
        Assert.AreEqual("name", view.userNameLabel.text);
        Assert.AreEqual(online, view.jumpInButtonContainer.activeSelf);
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
        view.OnPressBack += () => called = true;
        
        view.backButton.onClick.Invoke();
        
        Assert.IsTrue(called);
    }

    [Test]
    public void TriggerFocusWhenWindowIsClicked()
    {
        var clicked = false;
        view.OnClick += () => clicked = true;
        
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

    [UnityTest]
    public IEnumerator ActivatePreview()
    {
        view.ActivatePreview();

        yield return new WaitForSeconds(1f);

        foreach (var canvas in view.previewCanvasGroup)
            Assert.AreEqual(0f, canvas.alpha);
    }
    
    [UnityTest]
    public IEnumerator DeactivatePreview()
    {
        view.DeactivatePreview();

        yield return new WaitForSeconds(1f);

        foreach (var canvas in view.previewCanvasGroup)
            Assert.AreEqual(1f, canvas.alpha);
    }
}