using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SceneDataShould
{
    [Test]
    public void SceneDataCorrectSiz()
    {
        //Arrange
        DeployedScene scene = new DeployedScene();
        scene.parcelsCoord = new [] { new Vector2Int(1, 1) };

        //Act
        SceneData data = new SceneData(scene);

        //Assert
        Assert.AreSame(data.size, new Vector2Int(1, 1));
    }
}