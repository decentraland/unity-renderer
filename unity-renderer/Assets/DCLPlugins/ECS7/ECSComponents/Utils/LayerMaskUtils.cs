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

        public static bool IsInLayerMask(uint layerMask, int layer) => (layer & layerMask) != 0;

        public static bool LayerMaskHasAnySDKCustomLayer(uint layerMask) => (layerMask & ~nonCustomLayers) != 0;
    }
}
