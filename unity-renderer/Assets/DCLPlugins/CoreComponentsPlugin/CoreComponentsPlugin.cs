using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreComponentsPlugin : IPlugin
{
    public CoreComponentsPlugin()
    {
        // Transform
        // builders.Add((int) CLASS_ID_COMPONENT.TRANSFORM, BuildComponent<DCLTransform>);
        // builders.Add((int) CLASS_ID_COMPONENT.AVATAR_ATTACH, BuildComponent<AvatarAttachComponent>);

        // Shapes
        // builders.Add((int) CLASS_ID.BOX_SHAPE, BuildComponent<BoxShape>);
        // builders.Add((int) CLASS_ID.SPHERE_SHAPE, BuildComponent<SphereShape>);
        // builders.Add((int) CLASS_ID.CYLINDER_SHAPE, BuildComponent<CylinderShape>);
        // builders.Add((int) CLASS_ID.CONE_SHAPE, BuildComponent<ConeShape>);
        // builders.Add((int) CLASS_ID.PLANE_SHAPE, BuildComponent<PlaneShape>);
        // builders.Add((int) CLASS_ID.GLTF_SHAPE, BuildComponent<GLTFShape>);
        // builders.Add((int) CLASS_ID.OBJ_SHAPE, BuildComponent<OBJShape>);
        // builders.Add((int) CLASS_ID_COMPONENT.TEXT_SHAPE, BuildPoolableComponent);
        //
        // builders.Add((int) CLASS_ID_COMPONENT.BILLBOARD, BuildPoolableComponent);
        // builders.Add((int) CLASS_ID_COMPONENT.SMART_ITEM, BuildPoolableComponent);

        // Materials
        // builders.Add((int) CLASS_ID.BASIC_MATERIAL, BuildComponent<BasicMaterial>);
        // builders.Add((int) CLASS_ID.PBR_MATERIAL, BuildComponent<PBRMaterial>);
        // builders.Add((int) CLASS_ID.TEXTURE, BuildComponent<DCLTexture>);
        // builders.Add((int) CLASS_ID.AVATAR_TEXTURE, BuildComponent<DCLAvatarTexture>);

        // Audio
        // builders.Add((int) CLASS_ID.AUDIO_CLIP, BuildComponent<DCLAudioClip>);
        // builders.Add((int) CLASS_ID_COMPONENT.AUDIO_SOURCE, BuildPoolableComponent);
        // builders.Add((int) CLASS_ID_COMPONENT.AUDIO_STREAM, BuildPoolableComponent);

        // Video
        // builders.Add((int) CLASS_ID.VIDEO_CLIP, BuildComponent<DCLVideoClip>);
        // builders.Add((int) CLASS_ID.VIDEO_TEXTURE, BuildComponent<DCLVideoTexture>);

        // Builder in world
        // builders.Add((int) CLASS_ID.NAME, BuildComponent<DCLName>);
        // builders.Add((int) CLASS_ID.LOCKED_ON_EDIT, BuildComponent<DCLLockedOnEdit>);
        // builders.Add((int) CLASS_ID.VISIBLE_ON_EDIT, BuildComponent<DCLVisibleOnEdit>);

        // Others
        // builders.Add((int) CLASS_ID_COMPONENT.AVATAR_SHAPE, BuildPoolableComponent);
        // builders.Add((int) CLASS_ID_COMPONENT.ANIMATOR, BuildPoolableComponent);
        // builders.Add((int) CLASS_ID_COMPONENT.GIZMOS, BuildPoolableComponent);
        // builders.Add((int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA, BuildPoolableComponent);
        // builders.Add((int) CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION, BuildPoolableComponent);
        // builders.Add((int) CLASS_ID_COMPONENT.CAMERA_MODE_AREA, BuildComponent<CameraModeArea>);
    }

    public void Dispose()
    {
    }
}