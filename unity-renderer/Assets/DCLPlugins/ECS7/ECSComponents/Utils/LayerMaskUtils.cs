namespace DCL.ECSComponents.Utils
{
    public static class LayerMaskUtils
    {
        public static bool IsInLayerMask(uint layerMask, int layer) => (layer & layerMask) != 0;

        public static bool LayerMaskHasAnySDKCustomLayer(uint layerMask)
        {
            return IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom1)
                   || IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom2)
                   || IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom3)
                   || IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom4)
                   || IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom5)
                   || IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom6)
                   || IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom7)
                   || IsInLayerMask(layerMask, (int)ColliderLayer.ClCustom8);
        }
    }
}
