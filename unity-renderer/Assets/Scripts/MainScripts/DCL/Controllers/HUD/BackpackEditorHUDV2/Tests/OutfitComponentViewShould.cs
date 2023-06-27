using DCL.Backpack;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class OutfitComponentViewShould
{
    private OutfitComponentView view;

    [SetUp]
    public void SetUp()
    {
        view = Object.Instantiate(AssetDatabase.LoadAssetAtPath<OutfitComponentView>(
            "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Outfits/OutfitItem.prefab"));
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetIsEmpty(bool isEmpty)
    {
        view.SetIsEmpty(isEmpty);

        Assert.AreEqual(isEmpty, view.emptyState.activeSelf);
        Assert.AreEqual(!isEmpty, view.filledState.activeSelf);
    }


    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetIsLoading(bool isLoading)
    {
        view.SetIsLoading(isLoading);

        Assert.AreEqual(isLoading, view.loadingState.activeSelf);
    }

    [Test]
    public void ActivateCorrespondingOnFocus()
    {
        view.OnFocus();

        foreach (GameObject hoverState in view.hoverStates)
            Assert.IsTrue(hoverState.activeSelf);
        foreach (GameObject normalState in view.normalStates)
            Assert.IsFalse(normalState.activeSelf);
    }

    [Test]
    public void ActivateCorrespondingOnLostFocus()
    {
        view.OnLoseFocus();

        foreach (GameObject normalState in view.normalStates)
            Assert.IsTrue(normalState.activeSelf);
        foreach (GameObject hoverState in view.hoverStates)
            Assert.IsFalse(hoverState.activeSelf);
    }

}
