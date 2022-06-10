using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PublicChatChannelComponentViewShould
{
    private PublicChatChannelComponentView view;
    
    [SetUp]
    public void SetUp()
    {
        view = PublicChatChannelComponentView.Create();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(view.gameObject);
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
        view.Configure(new PublicChatChannelModel("nearby", "nearby", "any description"));
        
        Assert.AreEqual("#nearby", view.nameLabel.text);
        Assert.AreEqual("any description", view.descriptionLabel.text);
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
        var focused = false;
        view.OnFocused += f => focused = f;
        
        view.OnPointerDown(null);
        
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
    
    [Test]
    public void ActivatePreviewInstantly()
    {
        view.ActivatePreviewInstantly();

        foreach (var canvas in view.previewCanvasGroup)
            Assert.AreEqual(0f, canvas.alpha);
    }
}