using System;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.SettingsCommon;

public class ECS7ComponentsPlugin : IDisposable
{

    private readonly ECSTransformComponent transformComponent;
    private readonly ECSComponentsFactory componentsFactory;
    private readonly ECSTextShapeComponent textShapeComponent;

    public ECS7ComponentsPlugin(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
    {
        this.componentsFactory = componentsFactory;
        transformComponent = new ECSTransformComponent(ComponentID.TRANSFORM, componentsFactory, componentsWriter);
        textShapeComponent = new ECSTextShapeComponent(ComponentID.TEXT_SHAPE, componentsFactory, componentsWriter);
        
        RegisterComponents();
    }

        // Box Shape
        componentsFactory.AddOrReplaceComponent(ComponentID.BOX_SHAPE,
            data => PBBoxShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSBoxShapeComponentHandler(DataStore.i.ecs7));

        // Plane Shape
        componentsFactory.AddOrReplaceComponent(ComponentID.PLANE_SHAPE,
            data => PBPlaneShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSPlaneShapeComponentHandler());

        // Sphere Shape
        componentsFactory.AddOrReplaceComponent(ComponentID.SPHERE_SHAPE,
            data => PBSphereShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSSphereShapeComponentHandler());
        
        // Cylinder Shape
        componentsFactory.AddOrReplaceComponent(ComponentID.CYLINDER_SHAPE,
            data => PBCylinderShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSCylinderShapeComponentHandler());

        // Audio Source
        componentsFactory.AddOrReplaceComponent(ComponentID.AUDIO_SOURCE_SHAPE,
            data => PBAudioSource.Parser.ParseFrom((byte[])data),
            () =>  new ECSAudioSourceComponentHandler(DataStore.i, Settings.i, AssetPromiseKeeper_AudioClip.i, CommonScriptableObjects.sceneID));

        // Audio Stream
        componentsFactory.AddOrReplaceComponent(ComponentID.AUDIO_STREAM_SHAPE,
            data => PBAudioStream.Parser.ParseFrom((byte[])data),
            () =>  new ECSAudioStreamComponentHandler());
    }

    public void Dispose()
    {
        transformComponent.Dispose();
        componentsFactory.RemoveComponent(ComponentID.BOX_SHAPE);
        componentsFactory.RemoveComponent(ComponentID.BOX_SHAPE);
        componentsFactory.RemoveComponent(ComponentID.PLANE_SHAPE);
        componentsFactory.RemoveComponent(ComponentID.SPHERE_SHAPE);
        componentsFactory.RemoveComponent(ComponentID.CYLINDER_SHAPE);
        componentsFactory.RemoveComponent(ComponentID.AUDIO_SOURCE_SHAPE);
        componentsFactory.RemoveComponent(ComponentID.AUDIO_STREAM_SHAPE);
        textShapeComponent.Dispose();
    }
}