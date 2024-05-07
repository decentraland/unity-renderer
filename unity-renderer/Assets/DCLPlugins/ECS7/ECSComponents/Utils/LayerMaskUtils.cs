using DCL.Configuration;

namespace DCL.ECSComponents.Utils
{
    public static class LayerMaskUtils
    {
        private const int nonCustomLayers = (int)ColliderLayer.ClPhysics
                                            | (int)ColliderLayer.ClPointer
                                            | (int)ColliderLayer.ClNone
                                            | (int)ColliderLayer.ClReserved1
                                            | (int)ColliderLayer.ClReserved2
                                            | (int)ColliderLayer.ClReserved3
                                            | (int)ColliderLayer.ClReserved4
                                            | (int)ColliderLayer.ClReserved5
                                            | (int)ColliderLayer.ClReserved6;

        public static bool IsInLayerMask(uint layerMask, int layer) =>
            (layer & layerMask) != 0;

        public static bool LayerMaskHasAnySDKCustomLayer(uint layerMask) =>
            (layerMask & ~nonCustomLayers) != 0;

        public static int? SdkLayerMaskToUnityLayer(uint mask)
        {
            const int LAYER_PHYSICS = (int)ColliderLayer.ClPhysics;
            const int LAYER_POINTER = (int)ColliderLayer.ClPointer;
            const int LAYER_PHYSICS_POINTER = LAYER_PHYSICS | LAYER_POINTER;

            if ((mask & LAYER_PHYSICS_POINTER) == LAYER_PHYSICS_POINTER)
                return PhysicsLayers.defaultLayer;

            if ((mask & LAYER_PHYSICS) == LAYER_PHYSICS)
                return PhysicsLayers.characterOnlyLayer;

            if ((mask & LAYER_POINTER) == LAYER_POINTER)
                return PhysicsLayers.onPointerEventLayer;

            if (LayerMaskHasAnySDKCustomLayer(mask))
                return PhysicsLayers.sdkCustomLayer;

            return null;
        }
    }
}
