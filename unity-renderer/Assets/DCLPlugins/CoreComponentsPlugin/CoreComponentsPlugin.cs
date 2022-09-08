using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public class CoreComponentsPlugin : IPlugin
{
    private PoolableComponentFactory poolableComponentFactory;

    public CoreComponentsPlugin()
    {
        poolableComponentFactory = Resources.Load<PoolableComponentFactory>("PoolableCoreComponentsFactory");
        IRuntimeComponentFactory factory = Environment.i.world.componentFactory; 

        // Transform
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.TRANSFORM, BuildComponent<DCLTransform>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.AVATAR_ATTACH, BuildComponent<AvatarAttachComponent>);

        // Shapes
        factory.RegisterBuilder((int) CLASS_ID.BOX_SHAPE, BuildComponent<BoxShape>);
        factory.RegisterBuilder((int) CLASS_ID.SPHERE_SHAPE, BuildComponent<SphereShape>);
        factory.RegisterBuilder((int) CLASS_ID.CYLINDER_SHAPE, BuildComponent<CylinderShape>);
        factory.RegisterBuilder((int) CLASS_ID.CONE_SHAPE, BuildComponent<ConeShape>);
        factory.RegisterBuilder((int) CLASS_ID.PLANE_SHAPE, BuildComponent<PlaneShape>);
        factory.RegisterBuilder((int) CLASS_ID.GLTF_SHAPE, BuildComponent<GLTFShape>);
        factory.RegisterBuilder((int) CLASS_ID.OBJ_SHAPE, BuildComponent<OBJShape>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.TEXT_SHAPE,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.TEXT_SHAPE));
        //
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.BILLBOARD,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.BILLBOARD));
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.SMART_ITEM,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.SMART_ITEM));

        // Materials
        factory.RegisterBuilder((int) CLASS_ID.BASIC_MATERIAL, BuildComponent<BasicMaterial>);
        factory.RegisterBuilder((int) CLASS_ID.PBR_MATERIAL, BuildComponent<PBRMaterial>);
        factory.RegisterBuilder((int) CLASS_ID.TEXTURE, BuildComponent<DCLTexture>);
        factory.RegisterBuilder((int) CLASS_ID.AVATAR_TEXTURE, BuildComponent<DCLAvatarTexture>);

        // Audio
        factory.RegisterBuilder((int) CLASS_ID.AUDIO_CLIP, BuildComponent<DCLAudioClip>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.AUDIO_SOURCE,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.AUDIO_SOURCE));
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.AUDIO_STREAM,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.AUDIO_STREAM));

        // Video
        factory.RegisterBuilder((int) CLASS_ID.VIDEO_CLIP, BuildComponent<DCLVideoClip>);
        factory.RegisterBuilder((int) CLASS_ID.VIDEO_TEXTURE, BuildComponent<DCLVideoTexture>);
        
        // Others
        factory.RegisterBuilder((int) CLASS_ID.FONT, BuildComponent<DCLFont>);
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.AVATAR_SHAPE,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.AVATAR_SHAPE));
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.ANIMATOR,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.ANIMATOR));
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.GIZMOS,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.GIZMOS));
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA));
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION,
            () => BuildPoolableComponent((int) CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION));
        factory.RegisterBuilder((int) CLASS_ID_COMPONENT.CAMERA_MODE_AREA, BuildComponent<CameraModeArea>);

        factory.createOverrides.Add((int) CLASS_ID_COMPONENT.TRANSFORM, HandleAvatarAttachExclusivity);

        CoroutineStarter.Start(PrewarmPoolablePools());
    }

    IEnumerator PrewarmPoolablePools()
    {
        yield return null;
        poolableComponentFactory.PrewarmPools();
    }

    private void HandleAvatarAttachExclusivity(string sceneid, long entityid, ref int classid, object data)
    {
        // NOTE: TRANSFORM and AVATAR_ATTACH can't be used in the same Entity at the same time.
        // so we remove AVATAR_ATTACH (if exists) when a TRANSFORM is created.
        IParcelScene scene = Environment.i.world.state.GetScene(sceneid);
        IDCLEntity entity = scene.entities[entityid];

        if (scene.componentsManagerLegacy.TryGetBaseComponent(entity, CLASS_ID_COMPONENT.AVATAR_ATTACH, out IEntityComponent component))
        {
            component.Cleanup();
            scene.componentsManagerLegacy.RemoveComponent(entity, CLASS_ID_COMPONENT.AVATAR_ATTACH);
        }
    }

    protected T BuildComponent<T>()
        where T : IComponent, new()
    {
        return new T();
    }

    private IComponent BuildPoolableComponent(int classId)
    {
        return poolableComponentFactory.CreateItemFromId<BaseComponent>((CLASS_ID_COMPONENT) classId);
    }

    public void Dispose()
    {
        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;

        if (factory == null)
            return;
        
        // Transform
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.TRANSFORM);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.AVATAR_ATTACH);

        // Shapes
        factory.UnregisterBuilder((int) CLASS_ID.BOX_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.SPHERE_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.CYLINDER_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.CONE_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.PLANE_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.GLTF_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.OBJ_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.TEXT_SHAPE);
        //
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.BILLBOARD);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.SMART_ITEM);

        // Materials
        factory.UnregisterBuilder((int) CLASS_ID.BASIC_MATERIAL);
        factory.UnregisterBuilder((int) CLASS_ID.PBR_MATERIAL);
        factory.UnregisterBuilder((int) CLASS_ID.TEXTURE);
        factory.UnregisterBuilder((int) CLASS_ID.AVATAR_TEXTURE);

        // Audio
        factory.UnregisterBuilder((int) CLASS_ID.AUDIO_CLIP);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.AUDIO_SOURCE);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.AUDIO_STREAM);

        // Video
        factory.UnregisterBuilder((int) CLASS_ID.VIDEO_CLIP);
        factory.UnregisterBuilder((int) CLASS_ID.VIDEO_TEXTURE);

        // Builder in world
        factory.UnregisterBuilder((int) CLASS_ID.NAME);
        factory.UnregisterBuilder((int) CLASS_ID.LOCKED_ON_EDIT);
        factory.UnregisterBuilder((int) CLASS_ID.VISIBLE_ON_EDIT);

        // Others
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.AVATAR_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.ANIMATOR);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.GIZMOS);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        factory.UnregisterBuilder((int) CLASS_ID_COMPONENT.CAMERA_MODE_AREA);
        factory.UnregisterBuilder((int) CLASS_ID.FONT);


        factory.createOverrides.Remove((int) CLASS_ID_COMPONENT.TRANSFORM);
    }
}