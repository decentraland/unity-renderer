using DCLServices.MapRendererV2.ConsumerUtils;

namespace DCL
{
    public interface INavmapToastViewController
    {
        void CloseCurrentToast();

        void ShowPlaceToast(MapRenderImage.ParcelClickData parcelClickData, bool showUntilClick);
    }
}
