using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEditor;

[Category("EditModeCI")]
public class ToSPopupViewShould
{
    private ToSPopupView view;

    [SetUp]
    public void SetUp()
    {
        var prefab = new GameObject("ToSPopupView");

        view = Object.Instantiate(AssetDatabase
                         .LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/ToSPopupHUD/Prefabs/ToSPopupHUD.prefab")
                      )
                     .GetComponentInChildren<ToSPopupView>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(view.gameObject);
    }

    [Test]
    public void TestAcceptButtonPress()
    {
        bool wasAcceptButtonPressed = false;
        view.OnAccept += () => wasAcceptButtonPressed = true;

        view.agreeButton.onClick.Invoke();

        Assert.IsTrue(wasAcceptButtonPressed);
    }

    [Test]
    public void TestCancelButtonPress()
    {
        bool wasCancelButtonPressed = false;
        view.OnCancel += () => wasCancelButtonPressed = true;

        view.cancelButton.onClick.Invoke();

        Assert.IsTrue(wasCancelButtonPressed);
    }

    [Test]
    public void TestShow()
    {
        view.Show();

        Assert.IsTrue(view.gameObject.activeSelf);
    }
}
