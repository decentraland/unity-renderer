using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class BuilderInWorldShould
{
    private BuilderInWorldPlugin builderInWorld;
    
    [SetUp]
    public void SetUp()
    {
        //Arrange
        builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContextForTestScene());
        builderInWorld.editor = Substitute.For<IBIWEditor>();
        builderInWorld.sceneManager = Substitute.For<ISceneManager>();
        builderInWorld.panelController = Substitute.For<IBuilderMainPanelController>();
        builderInWorld.builderAPIController = Substitute.For<IBuilderAPIController>();
        builderInWorld.cameraController = Substitute.For<ICameraController>();
    }

    [TearDown]
    public void TearDown()
    {
        builderInWorld.Dispose();
    }
    
    [Test]
    public void CreateContextCorrectly()
    {
        //Assert
        Assert.IsNotNull(builderInWorld.context);
        Assert.IsNotNull(builderInWorld.context.editorContext);
    }

    [Test]
    public void InitializePartsCorrectly()
    {
        //Arrange
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContextForTestScene());

        //Assert
        builderInWorld.editor.Received(1).Initialize(builderInWorld.context);
    }

    [Test]
    public void DisposePartsCorrectly()
    {
        //Act
        builderInWorld.Dispose();

        //Assert
        builderInWorld.editor.Received(1).Dispose();
    }

    [Test]
    public void CallUpdateCorrectly()
    {
        //Act
        builderInWorld.Update();

        //Assert
        builderInWorld.editor.Received(1).Update();
    }

    [Test]
    public void CallLateUpdateCorrectly()
    {
        //Act
        builderInWorld.LateUpdate();

        //Assert
        builderInWorld.editor.Received(1).LateUpdate();
    }

    [Test]
    public void CallOnGUICorrectly()
    {
        //Act
        builderInWorld.OnGUI();

        //Assert
        builderInWorld.editor.Received(1).OnGUI();
    }
}