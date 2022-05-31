using System;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL;
using DCL.ECSComponents;
using UnityEngine;

public class ECS7ComponentsPlugin : IDisposable
{

    private readonly ECSTransformComponent transformComponent;

    public ECS7ComponentsPlugin(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
    {
        transformComponent = new ECSTransformComponent(1, componentsFactory, componentsWriter);
        RegisterComponents();
    }

    public void RegisterComponents()
    {
        DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.BOX_SHAPE,
            data => PBBoxShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSBoxShapeComponentHandler());
    }

    public void Dispose()
    {
        transformComponent.Dispose();
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.BOX_SHAPE);
    }
}