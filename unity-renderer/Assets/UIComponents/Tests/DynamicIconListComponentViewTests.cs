using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DynamicIconListComponentViewTests
{
    private DynamicListComponentView listComponentView;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        listComponentView = BaseComponentView.Create<DynamicListComponentView>("DynamicIconList");
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        listComponentView.Dispose();
        Object.Destroy(listComponentView.gameObject);
    }

    [Test]
    public void AddNewIcon()
    {
        listComponentView.AddIcon(testSprite);

        Assert.True(listComponentView.instantiatedIcons.Count == 1);
        Assert.True(listComponentView.transform.childCount == 3);
    }

    [Test]
    public void AddNewItems()
    {
        List<Sprite> sprites = new List<Sprite>() {testSprite,testSprite,testSprite,testSprite};

        listComponentView.AddIcons(sprites);

        Assert.True(listComponentView.instantiatedIcons.Count == 4);
        Assert.True(listComponentView.transform.childCount == 6);
    }

    [Test]
    public void RemoveItems()
    {
        List<Sprite> sprites = new List<Sprite>() {testSprite,testSprite,testSprite,testSprite};

        listComponentView.AddIcons(sprites);

        listComponentView.RemoveIcons();

        Assert.AreEqual(0, listComponentView.instantiatedIcons.Count);
    }
}
