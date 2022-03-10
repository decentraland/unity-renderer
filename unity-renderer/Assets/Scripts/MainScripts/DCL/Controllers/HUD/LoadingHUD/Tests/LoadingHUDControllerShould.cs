using DCL;
using LoadingHUD;
using NUnit.Framework;
using UnityEngine;

public class LoadingHUDControllerShould
{
    private LoadingHUDController hudController;
    private LoadingHUDView hudView;
    private BaseVariable<bool> visible => DataStore.i.HUDs.loadingHUD.visible;
    private BaseVariable<string> message => DataStore.i.HUDs.loadingHUD.message;
    private BaseVariable<bool> showTips => DataStore.i.HUDs.loadingHUD.showTips;

    [SetUp]
    public void SetUp()
    {
        hudController = new LoadingHUDController();
        hudController.Initialize();
        hudView = hudController.view;
    }

    [Test]
    public void InitializeProperly()
    {
        Assert.AreEqual(hudView, hudController.view);
    }

    [Test]
    public void ReactToLoadingHUDVisibleTrue()
    {
        visible.Set(true, true); //Force event notification
        Assert.AreEqual(hudView.gameObject.activeInHierarchy, true);
    }

    [Test]
    public void ReactToLoadingHUDVisibleFalse()
    {
        visible.Set(false, true); //Force event notification
        Assert.AreEqual(hudView.gameObject.activeInHierarchy, false);
    }

    [Test]
    public void ReactToMessageChanged()
    {
        message.Set("new_message", true); //Force event notification
        Assert.AreEqual(hudView.text.text, "new_message");
    }

    [Test]
    public void ReactToShowTipsChanged()
    {
        showTips.Set(false, true); //Force event notification
        Assert.AreEqual(hudView.tipsContainer.activeInHierarchy, false);
    }

    [TearDown]
    public void TearDown() { DataStore.Clear(); }
}