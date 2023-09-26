using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents.UIDropdown;
using DCL.ECSComponents.UIInput;
using DCL.ECSComponents.UIText;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents;
using System;
using System.Collections.Generic;

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
        private readonly TweenRegister tweenRegister;

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
        private readonly UiCanvasInformationRegister uiCanvasInformationRegister;
        private readonly TweenStateRegister tweenStateRegister;

        public ECS7ComponentsComposer(
            ECSComponentsFactory componentsFactory,
            IECSComponentWriter componentWriter,
            IInternalECSComponents internalComponents,
            WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter)
        {
            transformRegister = new TransformRegister(ComponentID.TRANSFORM, componentsFactory, componentWriter, internalComponents);
            audioStreamRegister = new AudioStreamRegister(ComponentID.AUDIO_STREAM, componentsFactory, componentWriter);
            audioSourceRegister = new AudioSourceRegister(ComponentID.AUDIO_SOURCE, componentsFactory, componentWriter, internalComponents);
            nftRegister = new NFTShapeRegister(ComponentID.NFT_SHAPE, componentsFactory, componentWriter, internalComponents);
            textShapeRegister = new ECSTextShapeRegister(ComponentID.TEXT_SHAPE, componentsFactory, componentWriter, internalComponents);
            gltfRegister = new GltfContainerRegister(ComponentID.GLTF_CONTAINER, componentsFactory, componentWriter, internalComponents);
            animatorRegister = new AnimatorRegister(ComponentID.ANIMATOR, componentsFactory, componentWriter);
            billboardRegister = new BillboardRegister(ComponentID.BILLBOARD, componentsFactory, componentWriter);
            avatarAttachRegister = new AvatarAttachRegister(ComponentID.AVATAR_ATTACH, componentsFactory, componentWriter, internalComponents);
            avatarModifierAreaRegister = new AvatarModifierAreaRegister(ComponentID.AVATAR_MODIFIER_AREA, componentsFactory, componentWriter);
            avatarShapeRegister = new AvatarShapeRegister(ComponentID.AVATAR_SHAPE, componentsFactory, componentWriter, internalComponents);
            cameraModeAreaRegister = new CameraModeAreaRegister(ComponentID.CAMERA_MODE_AREA, componentsFactory, componentWriter);
            materialRegister = new MaterialRegister(ComponentID.MATERIAL, componentsFactory, componentWriter, internalComponents);
            raycastRegister = new RaycastRegister(ComponentID.RAYCAST, componentsFactory, componentWriter, internalComponents);
            raycastResultRegister = new RaycastResultRegister(ComponentID.RAYCAST_RESULT, componentsFactory, componentWriter);
            meshRendererRegister = new MeshRendererRegister(ComponentID.MESH_RENDERER, componentsFactory, componentWriter, internalComponents);
            meshColliderRegister = new MeshColliderRegister(ComponentID.MESH_COLLIDER, componentsFactory, componentWriter, internalComponents);
            visibilityComponentRegister = new VisibilityComponentRegister(ComponentID.VISIBILITY_COMPONENT, componentsFactory, componentWriter, internalComponents);
            videoPlayerRegister = new VideoPlayerRegister(ComponentID.VIDEO_PLAYER, componentsFactory, componentWriter, internalComponents);
            videoEventRegister = new VideoEventRegister(ComponentID.VIDEO_EVENT, componentsFactory, componentWriter);
            tweenRegister = new TweenRegister(ComponentID.TWEEN, componentsFactory, componentWriter, internalComponents, tweenStateComponentPool, componentsWriter);

            // Multi-purposed components
            pointerEvents = new PointerEventsRegister(ComponentID.POINTER_EVENTS, componentsFactory, componentWriter, internalComponents.PointerEventsComponent);

            // UI components
            uiTransformRegister = new UITransformRegister(ComponentID.UI_TRANSFORM, componentsFactory, componentWriter, internalComponents.uiContainerComponent);
            uiTextRegister = new UiTextRegister(ComponentID.UI_TEXT, componentsFactory, componentWriter, internalComponents.uiContainerComponent);
            uiBackgroundRegister = new UIBackgroundRegister(ComponentID.UI_BACKGROUND, componentsFactory, componentWriter, internalComponents.uiContainerComponent);
            uiInputRegister = new UIInputRegister(ComponentID.UI_INPUT, ComponentID.UI_INPUT_RESULT, componentsFactory, componentWriter, internalComponents.uiContainerComponent, internalComponents.uiInputResultsComponent);
            uiDropdownRegister = new UIDropdownRegister(ComponentID.UI_DROPDOWN, ComponentID.UI_DROPDOWN_RESULT, componentsFactory, componentWriter, internalComponents.uiContainerComponent, internalComponents.uiInputResultsComponent);

            // Components without a handler
            pointerEventResultRegister = new PointerEventResultRegister(ComponentID.POINTER_EVENTS_RESULT, componentsFactory, componentWriter);
            cameraModeRegister = new CameraModeRegister(ComponentID.CAMERA_MODE, componentsFactory, componentWriter);
            pointerLockRegister = new PointerLockRegister(ComponentID.POINTER_LOCK, componentsFactory, componentWriter);
            gltfContainerLoadingStateRegister = new GltfContainerLoadingStateRegister(ComponentID.GLTF_CONTAINER_LOADING_STATE, componentsFactory, componentWriter);
            engineInfoRegister = new EngineInfoRegister(ComponentID.ENGINE_INFO, componentsFactory, componentWriter);
            uiCanvasInformationRegister = new UiCanvasInformationRegister(ComponentID.UI_CANVAS_INFORMATION, componentsFactory, componentWriter);
            tweenStateRegister = new TweenStateRegister(ComponentID.TWEEN_STATE, componentsFactory, componentWriter);
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
            tweenRegister.Dispose();

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
            uiCanvasInformationRegister.Dispose();
            tweenStateRegister.Dispose();
        }
    }
}
