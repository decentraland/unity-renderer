using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using NSubstitute;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BuilderInWorldShould : IntegrationTestSuite
{
    private BuilderInWorldPlugin builderInWorld;

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IUpdateEventHandler>(() => new UpdateEventHandler());
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        //Arrange
        builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContextForTestScene());
    }

    protected override IEnumerator TearDown()
    {
        builderInWorld.Dispose();
        yield return base.TearDown();
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void CreateContextCorrectly()
    {
        //Assert
        Assert.IsNotNull(builderInWorld.context);
        Assert.IsNotNull(builderInWorld.context.editorContext);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void InitializePartsCorrectly()
    {
        //Arrange
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContextForTestScene());

        //Assert
        builderInWorld.editor.Received(1).Initialize(builderInWorld.context);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void DisposePartsCorrectly()
    {
        //Act
        builderInWorld.Dispose();

        //Assert
        builderInWorld.editor.Received(1).Dispose();
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void CallUpdateCorrectly()
    {
        //Act
        builderInWorld.Update();

        //Assert
        builderInWorld.editor.Received(1).Update();
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void CallLateUpdateCorrectly()
    {
        //Act
        builderInWorld.LateUpdate();

        //Assert
        builderInWorld.editor.Received(1).LateUpdate();
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void CallOnGUICorrectly()
    {
        //Act
        builderInWorld.OnGUI();

        //Assert
        builderInWorld.editor.Received(1).OnGUI();
    }
}