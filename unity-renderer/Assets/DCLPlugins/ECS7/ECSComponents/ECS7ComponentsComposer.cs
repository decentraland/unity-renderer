using System;
using DCL.ECSRuntime;
using DCL.ECS7;
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
        private readonly GLTFShapeRegister gltfRegister;
        private readonly ECSTextShapeRegister textShapeRegister;
        private readonly NFTShapeRegister nftRegister;
        private readonly OnPointerDownRegister pointerDownRegister;
        private readonly OnPointerUpRegister pointerUpRegister;
        private readonly AnimatorRegister animatorRegister;
        private readonly BillboardRegister billboardRegister;
        private readonly CameraModeAreaRegister cameraModeAreaRegister;
        private readonly AvatarModifierAreaRegister avatarModifierAreaRegister;
        private readonly AvatarAttachRegister avatarAttachRegister;

        // Those components are only here to serialize over the wire, we don't need a handler for these
        private readonly OnPointerDownResultRegister pointerDownResultRegister;
        private readonly OnPointerUpResultRegister pointerUpResultRegister;

        public ECS7ComponentsComposer(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
        {
            transformRegister = new TransformRegister(ComponentID.TRANSFORM, componentsFactory, componentsWriter);
            sphereShapeRegister = new SphereShapeRegister(ComponentID.SPHERE_SHAPE, componentsFactory, componentsWriter);
            boxShapeRegister = new BoxShapeRegister(ComponentID.BOX_SHAPE, componentsFactory, componentsWriter);
            planeShapeRegister = new PlaneShapeRegister(ComponentID.PLANE_SHAPE, componentsFactory, componentsWriter);
            cylinderShapeRegister = new CylinderShapeRegister(ComponentID.CYLINDER_SHAPE, componentsFactory, componentsWriter);
            audioStreamRegister = new AudioStreamRegister(ComponentID.AUDIO_STREAM, componentsFactory, componentsWriter);
            audioSourceRegister = new AudioSourceRegister(ComponentID.AUDIO_SOURCE, componentsFactory, componentsWriter);
            nftRegister = new NFTShapeRegister(ComponentID.NFT_SHAPE, componentsFactory, componentsWriter);
            textShapeRegister = new ECSTextShapeRegister(ComponentID.TEXT_SHAPE, componentsFactory, componentsWriter);
            gltfRegister = new GLTFShapeRegister(ComponentID.GLTF_SHAPE, componentsFactory, componentsWriter);
            pointerDownRegister = new OnPointerDownRegister(ComponentID.ON_POINTER_DOWN, componentsFactory, componentsWriter);
            pointerUpRegister = new OnPointerUpRegister(ComponentID.ON_POINTER_UP, componentsFactory, componentsWriter);
            animatorRegister = new AnimatorRegister(ComponentID.ANIMATOR, componentsFactory, componentsWriter);
            billboardRegister = new BillboardRegister(ComponentID.BILLBOARD, componentsFactory, componentsWriter);
            avatarAttachRegister = new AvatarAttachRegister(ComponentID.AVATAR_ATTACH, componentsFactory, componentsWriter);
            avatarModifierAreaRegister = new AvatarModifierAreaRegister(ComponentID.AVATAR_MODIFIER_AREA, componentsFactory, componentsWriter);
            cameraModeAreaRegister = new CameraModeAreaRegister(ComponentID.CAMERA_MODE_AREA, componentsFactory, componentsWriter);
            
            // Components without a handler
            pointerDownResultRegister = new OnPointerDownResultRegister(ComponentID.ON_POINTER_DOWN_RESULT, componentsFactory, componentsWriter);
            pointerUpResultRegister = new OnPointerUpResultRegister(ComponentID.ON_POINTER_UP_RESULT, componentsFactory, componentsWriter);
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
            pointerDownRegister.Dispose();
            pointerUpRegister.Dispose();
            cameraModeAreaRegister.Dispose();
            
            // Components without a handler
            pointerDownResultRegister.Dispose();
            pointerUpResultRegister.Dispose();
        }
    }
}