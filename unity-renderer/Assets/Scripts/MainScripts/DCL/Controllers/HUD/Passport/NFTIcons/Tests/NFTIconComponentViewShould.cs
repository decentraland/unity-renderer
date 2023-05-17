using UnityEngine;
using NUnit.Framework;
using UnityEditor;

public class NFTIconComponentViewShould : MonoBehaviour
{
    private NFTIconComponentView nftIconComponentView;

    [SetUp]
    public void SetUp()
    {
        nftIconComponentView = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<NFTIconComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/Passport/Prefabs/NFTIcon.prefab"));
    }

    [TearDown]
    public void TearDown()
    {
        nftIconComponentView.Dispose();
        GameObject.Destroy(nftIconComponentView.gameObject);
    }

    [Test]
    public void FocusComponentCorrectly()
    {
        // Act
        nftIconComponentView.SetShowMarketplaceButton(true);
        nftIconComponentView.OnFocus();
        // Assert
        Assert.IsTrue(nftIconComponentView.isFocused, "The base component should be focused.");
        Assert.IsTrue(nftIconComponentView.marketplaceSection.activeSelf, "The marketplace subpanel should be visible");
    }

    [Test]
    public void UnFocusComponentCorrectly()
    {
        // Act
        nftIconComponentView.SetShowMarketplaceButton(true);
        nftIconComponentView.OnLoseFocus();

        // Assert
        Assert.IsTrue(!nftIconComponentView.isFocused, "The base component should be un-focused.");
        Assert.IsTrue(!nftIconComponentView.marketplaceSection.activeSelf, "The marketplace subpanel should be hidden");
    }

    [Test]
    public void SetNameCorrectly()
    {
        // Act
        nftIconComponentView.SetName("TestNFTName");

        // Assert
        Assert.AreEqual(nftIconComponentView.model.name, "TestNFTName");
        Assert.AreEqual(nftIconComponentView.nftName.text, "TestNFTName");
        Assert.AreEqual(nftIconComponentView.nftNameMarketPlace.text, "TestNFTName");
    }

    [Test]
    public void SerMarketplaceURICorrectly()
    {
        // Act
        nftIconComponentView.SetMarketplaceURI("atesturi");

        // Assert
        Assert.AreEqual(nftIconComponentView.model.marketplaceURI, "atesturi");
    }
}
