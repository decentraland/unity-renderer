using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using UnityEngine;

public class SceneDataShould
{
    [Test]
    public void SceneDataShoulSetCorrectSize()
    {
        //Arrange
        var metadata = new CatalystSceneEntityPayload();
        metadata.metadata  = new CatalystSceneEntityMetadata();
        metadata.metadata.scene = new CatalystSceneEntityMetadata.Scene();
        metadata.metadata.scene.@base = "0,0";
        metadata.metadata.scene.parcels = new string[] { "0,0" };
        metadata.metadata.display = new CatalystSceneEntityMetadata.Display();
        metadata.metadata.display.navmapThumbnail = "TestURl";
        metadata.metadata.contact = new CatalystSceneEntityMetadata.Contact();
        metadata.metadata.contact.name = "";
        Scene scene = new Scene(metadata, "TestURL");
        scene.parcelsCoord = new [] { new Vector2Int(1, 1) };

        //Act
        SceneData data = new SceneData(scene);

        //Assert
        Assert.AreEqual(data.size, new Vector2Int(1, 1));
    }
}