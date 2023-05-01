using NSubstitute;
using NUnit.Framework;
using DCL.Social.Passports;
using DCL;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class PassportHUDViewShould
{
    private PlayerPassportHUDView view;

    [SetUp]
    public void SetUp()
    {
        view = AssetDatabase.LoadAssetAtPath<PlayerPassportHUDView>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Passport/Prefabs/PlayerPassport.prefab");
        Object.Instantiate(view.gameObject);
        view.Initialize(Substitute.For<MouseCatcher>());
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
    }

    [Test]
    public void Show()
    {
        view.SetVisibility(true);
        Assert.IsTrue(view.gameObject.activeSelf);
    }

    [Test]
    public void Hide()
    {
        view.SetVisibility(false);
        Assert.IsFalse(view.gameObject.activeSelf);
    }

    [Test]
    public void SetPassportPanelVisibilityTrue()
    {
        view.SetVisibility(true);
        view.SetPassportPanelVisibility(true);

        Assert.IsTrue(view.gameObject.activeSelf);
        Assert.IsTrue(view.container.activeSelf);
    }

    [Test]
    public void SetPassportPanelVisibilityFalse()
    {
        view.SetVisibility(true);
        view.SetPassportPanelVisibility(false);

        Assert.IsTrue(view.gameObject.activeSelf);
    }

}
