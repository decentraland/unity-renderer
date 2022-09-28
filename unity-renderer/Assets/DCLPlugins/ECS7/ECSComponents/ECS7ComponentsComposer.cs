using System;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents;

namespace DCL.ECSComponents
{
    public class ECS7ComponentsComposer : IDisposable
    {
        private readonly TransformRegister transformRegister;
        private readonly SphereShapeRegister sphereShapeRegister;
        private readonly BoxShapeRegister boxShapeRegister;
        private readonly PlaneShapeRegister planeShapeRegister;
        private readonly CylinderShapeRegister cylinderShapeRegister;
        private readonly AudioStreamRegister audioStreamRegister;
        private readonly AudioSourceRegister audioSourceRegister;
        private readonly GltfContainerRegister gltfRegister;
        private readonly ECSTextShapeRegister textShapeRegister;
        private readonly NFTShapeRegister nftRegister;
        private readonly AnimatorRegister animatorRegister;
        private readonly BillboardRegister billboardRegister;
        private readonly AvatarShapeRegister avatarShapeRegister;
        private readonly CameraModeAreaRegister cameraModeAreaRegister;
        private readonly AvatarModifierAreaRegister avatarModifierAreaRegister;
        private readonly AvatarAttachRegister avatarAttachRegister;
        private readonly MaterialRegister materialRegister;
        private readonly RaycastRegister raycastRegister;
        private readonly RaycastResultRegister raycastResultRegister;
        private readonly MeshRendererRegister meshRendererRegister;
        private readonly MeshColliderRegister meshColliderRegister;
        private readonly VisibilityComponentRegister visibilityComponentRegister;
        private readonly PointerEventsRegister pointerEventsRegister;

        // UI components
        private readonly UITransformRegister uiTransformRegister;
        private readonly UITextRegister uiTextRegister;

        // Those components are only here to serialize over the wire, we don't need a handler for these
        private readonly PointerEventResultRegister pointerEventResultRegister;
        private readonly CameraModeRegister cameraModeRegister;
        private readonly PointerLockRegister pointerLockRegister;

        public ECS7ComponentsComposer(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter, IInternalECSComponents internalComponents)
        {
            transformRegister = new TransformRegister(ComponentID.TRANSFORM, componentsFactory, componentsWriter);
            sphereShapeRegister = new SphereShapeRegister(ComponentID.SPHERE_SHAPE, componentsFactory, componentsWriter, internalComponents.texturizableComponent);
            boxShapeRegister = new BoxShapeRegister(ComponentID.BOX_SHAPE, componentsFactory, componentsWriter, internalComponents.texturizableComponent);
            planeShapeRegister = new PlaneShapeRegister(ComponentID.PLANE_SHAPE, componentsFactory, componentsWriter, internalComponents.texturizableComponent);
            cylinderShapeRegister = new CylinderShapeRegister(ComponentID.CYLINDER_SHAPE, componentsFactory, componentsWriter, internalComponents.texturizableComponent);
            audioStreamRegister = new AudioStreamRegister(ComponentID.AUDIO_STREAM, componentsFactory, componentsWriter);
            audioSourceRegister = new AudioSourceRegister(ComponentID.AUDIO_SOURCE, componentsFactory, componentsWriter);
            nftRegister = new NFTShapeRegister(ComponentID.NFT_SHAPE, componentsFactory, componentsWriter, internalComponents);
            textShapeRegister = new ECSTextShapeRegister(ComponentID.TEXT_SHAPE, componentsFactory, componentsWriter);
            gltfRegister = new GltfContainerRegister(ComponentID.GLTF_CONTAINER, componentsFactory, componentsWriter, internalComponents);
            animatorRegister = new AnimatorRegister(ComponentID.ANIMATOR, componentsFactory, componentsWriter);
            billboardRegister = new BillboardRegister(ComponentID.BILLBOARD, componentsFactory, componentsWriter);
            avatarAttachRegister = new AvatarAttachRegister(ComponentID.AVATAR_ATTACH, componentsFactory, componentsWriter);
            avatarModifierAreaRegister = new AvatarModifierAreaRegister(ComponentID.AVATAR_MODIFIER_AREA, componentsFactory, componentsWriter);
            avatarShapeRegister = new AvatarShapeRegister(ComponentID.AVATAR_SHAPE, componentsFactory, componentsWriter);
            cameraModeAreaRegister = new CameraModeAreaRegister(ComponentID.CAMERA_MODE_AREA, componentsFactory, componentsWriter);
            materialRegister = new MaterialRegister(ComponentID.MATERIAL, componentsFactory, componentsWriter, internalComponents);
            raycastRegister = new RaycastRegister(ComponentID.RAYCAST, componentsFactory, componentsWriter, internalComponents);
            raycastResultRegister = new RaycastResultRegister(ComponentID.RAYCAST_RESULT, componentsFactory, componentsWriter);
            meshRendererRegister = new MeshRendererRegister(ComponentID.MESH_RENDERER, componentsFactory, componentsWriter, internalComponents);
            meshColliderRegister = new MeshColliderRegister(ComponentID.MESH_COLLIDER, componentsFactory, componentsWriter, internalComponents);
            visibilityComponentRegister = new VisibilityComponentRegister(ComponentID.VISIBILITY_COMPONENT, componentsFactory, componentsWriter, internalComponents);

            // UI components
            uiTransformRegister = new UITransformRegister(ComponentID.UI_TRANSFORM, componentsFactory, componentsWriter);
            uiTextRegister = new UITextRegister(ComponentID.UI_TEXT, componentsFactory, componentsWriter);

            // Components without a handler
            pointerEventResultRegister = new PointerEventResultRegister(ComponentID.POINTER_EVENTS_RESULT, componentsFactory, componentsWriter);
            cameraModeRegister = new CameraModeRegister(ComponentID.CAMERA_MODE, componentsFactory, componentsWriter);
            pointerLockRegister = new PointerLockRegister(ComponentID.POINTER_LOCK, componentsFactory, componentsWriter);
            pointerEventsRegister = new PointerEventsRegister(ComponentID.POINTER_EVENTS, componentsFactory, componentsWriter);
        }

        public void Dispose()
        {
            transformRegister.Dispose();
            sphereShapeRegister.Dispose();
            boxShapeRegister.Dispose();
            billboardRegister.Dispose();
            planeShapeRegister.Dispose();
            cylinderShapeRegister.Dispose();
            audioStreamRegister.Dispose();
            audioSourceRegister.Dispose();
            textShapeRegister.Dispose();
            nftRegister.Dispose();
            gltfRegister.Dispose();
            animatorRegister.Dispose();
            avatarAttachRegister.Dispose();
            avatarModifierAreaRegister.Dispose();
            avatarShapeRegister.Dispose();
            cameraModeAreaRegister.Dispose();
            materialRegister.Dispose();
            raycastRegister.Dispose();
            raycastResultRegister.Dispose();
            meshRendererRegister.Dispose();
            meshColliderRegister.Dispose();
            visibilityComponentRegister.Dispose();

            // UI components
            uiTransformRegister.Dispose();
            uiTextRegister.Dispose();

            // Components without a handler
            pointerEventResultRegister.Dispose();
            cameraModeRegister.Dispose();
            pointerLockRegister.Dispose();
            pointerEventsRegister.Dispose();
        }
    }
}