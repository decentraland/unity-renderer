using DCL.ECS7;
using DCL.ECSComponents.UIDropdown;
using DCL.ECSComponents.UIInput;
using DCL.ECSComponents.UIText;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents;
using System;

namespace DCL.ECSComponents
{
    public class ECS7ComponentsComposer : IDisposable
    {
        private readonly TransformRegister transformRegister;
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
        private readonly PointerEventsRegister pointerEvents;
        private readonly VideoPlayerRegister videoPlayerRegister;

        // UI components
        private readonly UITransformRegister uiTransformRegister;
        private readonly UiTextRegister uiTextRegister;
        private readonly UIBackgroundRegister uiBackgroundRegister;
        private readonly UIInputRegister uiInputRegister;
        private readonly UIDropdownRegister uiDropdownRegister;

        // Those components are only here to serialize over the wire, we don't need a handler for these
        private readonly PointerEventResultRegister pointerEventResultRegister;
        private readonly CameraModeRegister cameraModeRegister;
        private readonly PointerLockRegister pointerLockRegister;
        private readonly VideoEventRegister videoEventRegister;
        private readonly GltfContainerLoadingStateRegister gltfContainerLoadingStateRegister;
        private readonly EngineInfoRegister engineInfoRegister;

        public ECS7ComponentsComposer(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter, IInternalECSComponents internalComponents)
        {
            transformRegister = new TransformRegister(ComponentID.TRANSFORM, componentsFactory, componentsWriter, internalComponents);
            audioStreamRegister = new AudioStreamRegister(ComponentID.AUDIO_STREAM, componentsFactory, componentsWriter);
            audioSourceRegister = new AudioSourceRegister(ComponentID.AUDIO_SOURCE, componentsFactory, componentsWriter, internalComponents);
            nftRegister = new NFTShapeRegister(ComponentID.NFT_SHAPE, componentsFactory, componentsWriter, internalComponents);
            textShapeRegister = new ECSTextShapeRegister(ComponentID.TEXT_SHAPE, componentsFactory, componentsWriter, internalComponents);
            gltfRegister = new GltfContainerRegister(ComponentID.GLTF_CONTAINER, componentsFactory, componentsWriter, internalComponents);
            animatorRegister = new AnimatorRegister(ComponentID.ANIMATOR, componentsFactory, componentsWriter);
            billboardRegister = new BillboardRegister(ComponentID.BILLBOARD, componentsFactory, componentsWriter);
            avatarAttachRegister = new AvatarAttachRegister(ComponentID.AVATAR_ATTACH, componentsFactory, componentsWriter, internalComponents);
            avatarModifierAreaRegister = new AvatarModifierAreaRegister(ComponentID.AVATAR_MODIFIER_AREA, componentsFactory, componentsWriter);
            avatarShapeRegister = new AvatarShapeRegister(ComponentID.AVATAR_SHAPE, componentsFactory, componentsWriter, internalComponents);
            cameraModeAreaRegister = new CameraModeAreaRegister(ComponentID.CAMERA_MODE_AREA, componentsFactory, componentsWriter);
            materialRegister = new MaterialRegister(ComponentID.MATERIAL, componentsFactory, componentsWriter, internalComponents);
            raycastRegister = new RaycastRegister(ComponentID.RAYCAST, componentsFactory, componentsWriter, internalComponents);
            raycastResultRegister = new RaycastResultRegister(ComponentID.RAYCAST_RESULT, componentsFactory, componentsWriter);
            meshRendererRegister = new MeshRendererRegister(ComponentID.MESH_RENDERER, componentsFactory, componentsWriter, internalComponents);
            meshColliderRegister = new MeshColliderRegister(ComponentID.MESH_COLLIDER, componentsFactory, componentsWriter, internalComponents);
            visibilityComponentRegister = new VisibilityComponentRegister(ComponentID.VISIBILITY_COMPONENT, componentsFactory, componentsWriter, internalComponents);
            videoPlayerRegister = new VideoPlayerRegister(ComponentID.VIDEO_PLAYER, componentsFactory, componentsWriter, internalComponents);
            videoEventRegister = new VideoEventRegister(ComponentID.VIDEO_EVENT, componentsFactory, componentsWriter);

            // Multi-purposed components
            pointerEvents = new PointerEventsRegister(ComponentID.POINTER_EVENTS, componentsFactory, componentsWriter, internalComponents.PointerEventsComponent);

            // UI components
            uiTransformRegister = new UITransformRegister(ComponentID.UI_TRANSFORM, componentsFactory, componentsWriter, internalComponents.uiContainerComponent);
            uiTextRegister = new UiTextRegister(ComponentID.UI_TEXT, componentsFactory, componentsWriter, internalComponents.uiContainerComponent);
            uiBackgroundRegister = new UIBackgroundRegister(ComponentID.UI_BACKGROUND, componentsFactory, componentsWriter, internalComponents.uiContainerComponent);
            uiInputRegister = new UIInputRegister(ComponentID.UI_INPUT, ComponentID.UI_INPUT_RESULT, componentsFactory, componentsWriter, internalComponents.uiContainerComponent, internalComponents.uiInputResultsComponent);
            uiDropdownRegister = new UIDropdownRegister(ComponentID.UI_DROPDOWN, ComponentID.UI_DROPDOWN_RESULT, componentsFactory, componentsWriter, internalComponents.uiContainerComponent, internalComponents.uiInputResultsComponent);

            // Components without a handler
            pointerEventResultRegister = new PointerEventResultRegister(ComponentID.POINTER_EVENTS_RESULT, componentsFactory, componentsWriter);
            cameraModeRegister = new CameraModeRegister(ComponentID.CAMERA_MODE, componentsFactory, componentsWriter);
            pointerLockRegister = new PointerLockRegister(ComponentID.POINTER_LOCK, componentsFactory, componentsWriter);
            gltfContainerLoadingStateRegister = new GltfContainerLoadingStateRegister(ComponentID.GLTF_CONTAINER_LOADING_STATE, componentsFactory, componentsWriter);
            engineInfoRegister = new EngineInfoRegister(ComponentID.ENGINE_INFO, componentsFactory, componentsWriter);
        }

        public void Dispose()
        {
            transformRegister.Dispose();
            billboardRegister.Dispose();
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
            videoPlayerRegister.Dispose();

            // UI components
            uiTransformRegister.Dispose();
            uiTextRegister.Dispose();
            uiBackgroundRegister.Dispose();
            uiInputRegister.Dispose();
            uiDropdownRegister.Dispose();

            // Components without a handler
            pointerEventResultRegister.Dispose();
            cameraModeRegister.Dispose();
            pointerLockRegister.Dispose();
            pointerEvents.Dispose();
            gltfContainerLoadingStateRegister.Dispose();
            videoEventRegister.Dispose();
            engineInfoRegister.Dispose();
        }
    }
}
