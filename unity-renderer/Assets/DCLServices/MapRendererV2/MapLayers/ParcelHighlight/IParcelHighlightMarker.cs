using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.ParcelHighlight
{
    internal interface IParcelHighlightMarker : IDisposable
    {
        void SetPosition(Vector3 position);

        void Activate();

        void Deactivate();
    }
}
