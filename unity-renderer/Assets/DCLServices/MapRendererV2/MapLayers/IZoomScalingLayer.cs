namespace DCLServices.MapRendererV2.MapLayers
{
    public interface IZoomScalingLayer
    {
        void ApplyCameraZoom(float baseZoom, float newZoom);

        void ResetToBaseScale();
    }
}
