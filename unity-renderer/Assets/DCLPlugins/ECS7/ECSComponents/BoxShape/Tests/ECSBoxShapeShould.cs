using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

public class ECSBoxShapeShould
{
    private IDCLEntity entity;
    private IParcelScene scene;
    private ECSComponentsManager componentsManager;
    private ECSBoxShapeComponentHandler boxShapeComponentHandler;

    [SetUp]
    public void SetUp()
    {
        entity = Substitute.For<IDCLEntity>();
        scene = Substitute.For<IParcelScene>();
        Dictionary<int, ECSComponentsFactory.ECSComponentBuilder> components =
            new Dictionary<int, ECSComponentsFactory.ECSComponentBuilder>()
            {
                {
                    (int)CLASS_ID.BOX_SHAPE,
                    ECSComponentsFactory.CreateComponentBuilder(ECSBoxShapeSerialization.Deserialize, () => boxShapeComponentHandler)
                }
            };

        componentsManager = new ECSComponentsManager(scene, components);
        entity.entityId.Returns(1);
        scene.sceneData.id.Configure().Returns("1");
    }

    [TearDown]
    public void TearDown()
    {
        
    }

    [Test]
    public void CreateComponentCorrectly()
    {
        IDCLEntity entity = Substitute.For<IDCLEntity>();
        entity.entityId.Returns(1);

        IECSComponent comp0 = componentsManager.GetOrCreateComponent((int)CLASS_ID.BOX_SHAPE, entity);
        // IECSComponent comp1 = componentsManager.GetOrCreateComponent((int)ComponentsID.Component1, entity);

    }
}
