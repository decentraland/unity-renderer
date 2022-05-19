using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL;
using DCL.ECSComponents;
using UnityEngine;

public class ECS7ComponentsPlugin : IPlugin
{
    public ECS7ComponentsPlugin()
    {
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
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.BOX_SHAPE);
    }
}
