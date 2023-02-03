using DCL;
using DCL.Providers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class LoadingHUDControllerShould
{
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

    private LoadingHUDController hudController;
    private LoadingHUDView hudView;
    private BaseVariable<bool> fadeIn => dataStoreLoadingScreen.Ref.loadingHUD.fadeIn;
    private BaseVariable<bool> fadeOut => dataStoreLoadingScreen.Ref.loadingHUD.fadeOut;
    private BaseVariable<string> message => dataStoreLoadingScreen.Ref.loadingHUD.message;
    private BaseVariable<bool> showTips => dataStoreLoadingScreen.Ref.loadingHUD.showTips;

    [SetUp]
    public async void SetUp()
    {
        hudView = Object.Instantiate(
                             AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/LoadingHUD/Prefabs/LoadingHUD.prefab"))
                        .GetComponent<LoadingHUDView>();

        hudController = new LoadingHUDController(hudView);
        hudController.Initialize();
    }

    [Test]
    public void InitializeProperly()
    {
        Assert.AreEqual(hudView, hudController.view);
    }

    [Test]
    public void ReactToLoadingHUDVisibleTrue()
    {
        fadeIn.Set(true, true); //Force event notification
        Assert.AreEqual(hudView.showHideAnimator.isVisible, true);
    }

    [Test]
    public void ReactToLoadingHUDVisibleFalse()
    {
        fadeOut.Set(false, true); //Force event notification
        Assert.AreEqual(hudView.showHideAnimator.isVisible, false);
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
