using System;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL;
using DCL.ECS7;
using DCL.ECSComponents;

public class ECS7ComponentsPlugin : IDisposable
{

    private readonly ECSTransformComponent transformComponent;

    public ECS7ComponentsPlugin(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
    {
        transformComponent = new ECSTransformComponent(1, componentsFactory, componentsWriter);
        RegisterComponents();
        
        // Text Shape
        DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.TEXT_SHAPE,
            data => PBTextShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSTextShapeComponentHandler(DataStore.i.ecs7));
        componentsWriter.AddOrReplaceComponentSerializer<PBTextShape>(ComponentID.TEXT_SHAPE, Serializer.Seri);
    }

    public void RegisterComponents()
    {
        DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.BOX_SHAPE,
            data => PBBoxShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSBoxShapeComponentHandler(DataStore.i.ecs7));
    }

    public void Dispose()
    {
        transformComponent.Dispose();
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.BOX_SHAPE);
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.TEXT_SHAPE);
    }
}