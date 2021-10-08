using System.Collections;
using System.Collections.Generic;
using DCL;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class BuilderInWorldShould
{
    [Test]
    public void CreateContextCorrectly()
    {
        //Act
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContext());

        //Assert
        Assert.IsNotNull(builderInWorld.context);
        Assert.IsNotNull(builderInWorld.context.editorContext);
    }

    [Test]
    public void InitializePartsCorrectly()
    {
        //Arrange
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContext());
        builderInWorld.editor = Substitute.For<IBIWEditor>();
        builderInWorld.panelController = Substitute.For<IBuilderProjectsPanelController>();

        //Act
        builderInWorld.Initialize();

        //Arrange
        builderInWorld.editor.Received(1).Initialize(builderInWorld.context);
    }

    [Test]
    public void DisposePartsCorrectly()
    {
        //Arrange
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContext());
        builderInWorld.editor = Substitute.For<IBIWEditor>();
        builderInWorld.panelController = Substitute.For<IBuilderProjectsPanelController>();

        //Act
        builderInWorld.Dispose();

        //Arrange
        builderInWorld.editor.Received(1).Dispose();
    }

    [Test]
    public void CallUpdateCorrectly()
    {
        //Arrange
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContext());
        builderInWorld.editor = Substitute.For<IBIWEditor>();
        builderInWorld.panelController = Substitute.For<IBuilderProjectsPanelController>();

        //Act
        builderInWorld.Update();

        //Arrange
        builderInWorld.editor.Received(1).Update();
    }

    [Test]
    public void CallLateUpdateCorrectly()
    {
        //Arrange
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContext());
        builderInWorld.editor = Substitute.For<IBIWEditor>();
        builderInWorld.panelController = Substitute.For<IBuilderProjectsPanelController>();

        //Act
        builderInWorld.LateUpdate();

        //Arrange
        builderInWorld.editor.Received(1).LateUpdate();
    }

    [Test]
    public void CallOnGUICorrectly()
    {
        //Arrange
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContext());
        builderInWorld.editor = Substitute.For<IBIWEditor>();
        builderInWorld.panelController = Substitute.For<IBuilderProjectsPanelController>();

        //Act
        builderInWorld.OnGUI();

        //Arrange
        builderInWorld.editor.Received(1).OnGUI();
    }
}