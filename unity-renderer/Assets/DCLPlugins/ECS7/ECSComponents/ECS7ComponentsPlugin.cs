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

    public ECS7ComponentsPlugin(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
    {
        transformComponent = new ECSTransformComponent(1, componentsFactory, componentsWriter);
        RegisterComponents();
    }

    public void RegisterComponents()
    {
        // Box Shape
        DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.BOX_SHAPE,
            data => PBBoxShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSBoxShapeComponentHandler());
        
        // Plane Shape
        DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.PLANE_SHAPE,
            data => PBPlaneShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSPlaneShapeComponentHandler());
        
        // Sphere Shape
        DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.SPHERE_SHAPE,
            data => PBSphereShape.Parser.ParseFrom((byte[])data),
            () =>  new ECSSphereShapeComponentHandler());
        
        // Audio Source
         DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.AUDIO_SOURCE_SHAPE,
                    data => PBAudioSource.Parser.ParseFrom((byte[])data),
                    () =>  new ECSAudioSourceComponentHandler(DataStore.i, Settings.i, AssetPromiseKeeper_AudioClip.i, CommonScriptableObjects.sceneID));
        
        // Audio Stream
         DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(ComponentID.AUDIO_STREAM_SHAPE,
                    data => PBAudioStream.Parser.ParseFrom((byte[])data),
                    () =>  new ECSAudioStreamComponentHandler());
    }

    public void Dispose()
    {
        transformComponent.Dispose();
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.BOX_SHAPE);
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.PLANE_SHAPE);
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.SPHERE_SHAPE);
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.AUDIO_SOURCE_SHAPE);
        DataStore.i.ecs7.componentsFactory.RemoveComponent(ComponentID.AUDIO_STREAM_SHAPE);
    }
}