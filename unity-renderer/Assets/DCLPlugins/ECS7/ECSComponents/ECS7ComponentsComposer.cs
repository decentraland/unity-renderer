using System;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.SettingsCommon;
using DCLPlugins.ECS7.ECSComponents.Events.OnPointerDown;

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

        public ECS7ComponentsComposer(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
        {
            transformRegister = new TransformRegister(ComponentID.TRANSFORM, componentsFactory, componentsWriter);
            sphereShapeRegister = new SphereShapeRegister(ComponentID.SPHERE_SHAPE, componentsFactory, componentsWriter);
            boxShapeRegister = new BoxShapeRegister(ComponentID.BOX_SHAPE, componentsFactory, componentsWriter);
            planeShapeRegister = new PlaneShapeRegister(ComponentID.PLANE_SHAPE, componentsFactory, componentsWriter);
            cylinderShapeRegister = new CylinderShapeRegister(ComponentID.CYLINDER_SHAPE, componentsFactory, componentsWriter);
            audioStreamRegister = new AudioStreamRegister(ComponentID.AUDIO_STREAM, componentsFactory, componentsWriter);
            audioSourceRegister = new AudioSourceRegister(ComponentID.AUDIO_SOURCE, componentsFactory, componentsWriter);
            nftRegister = new NFTShapeRegister(ComponentID.N_F_T_SHAPE, componentsFactory, componentsWriter);
            textShapeRegister = new ECSTextShapeRegister(ComponentID.TEXT_SHAPE, componentsFactory, componentsWriter);
            gltfRegister = new GLTFShapeRegister(ComponentID.G_L_T_F_SHAPE, componentsFactory, componentsWriter);
            pointerDownRegister = new OnPointerDownRegister(ComponentID.ON_POINTER_DOWN, componentsFactory, componentsWriter);
        }

        public void Dispose()
        {
            transformRegister.Dispose();
            sphereShapeRegister.Dispose();
            boxShapeRegister.Dispose();
            planeShapeRegister.Dispose();
            cylinderShapeRegister.Dispose();
            audioStreamRegister.Dispose();
            audioSourceRegister.Dispose();
            textShapeRegister.Dispose();
            gltfRegister.Dispose();
            nftRegister.Dispose();
            pointerDownRegister.Dispose();
        }
    }
}