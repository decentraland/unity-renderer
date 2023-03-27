using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public interface IMapInteractivityController
    {
        bool HighlightEnabled { get; }

        /// <summary>
        /// Highlights the (hovered) parcel
        /// </summary>
        void HighlightParcel(Vector2Int parcel);

        void RemoveHighlight();

        /// <summary>
        /// Returns Parcel corresponding to the given (cursor) position within UI `RawImage`
        /// </summary>
        Vector2Int GetParcel(Vector2 normalizedCoordinates);

        Vector2 GetNormalizedPosition(Vector2Int parcel);
    }
}
