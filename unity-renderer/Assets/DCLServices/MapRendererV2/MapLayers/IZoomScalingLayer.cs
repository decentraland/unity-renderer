namespace DCLServices.MapRendererV2.MapLayers
{
    public interface IZoomScalingLayer
    {
        void ApplyCameraZoom(float newZoom);

        void ResetToBaseScale();
    }
}
