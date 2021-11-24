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

    protected override PlatformContext CreatePlatformContext()
    {
        return DCL.Tests.PlatformContextFactory.CreateWithGenericMocks(new UpdateEventHandler());
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        //Arrange
        builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContextForTestScene(), Substitute.For<ISceneManager>());
    }

    protected override IEnumerator TearDown()
    {
        builderInWorld.Dispose();
        yield return base.TearDown();
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
        BuilderInWorldPlugin builderInWorld = new BuilderInWorldPlugin(BIWTestUtils.CreateMockedContextForTestScene(), Substitute.For<ISceneManager>());

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