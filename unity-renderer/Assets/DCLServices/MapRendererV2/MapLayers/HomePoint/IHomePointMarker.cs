using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.HomePoint
{
    internal interface IHomePointMarker : IDisposable
    {
        void SetPosition(Vector3 position);

        void SetActive(bool active);
    }
}
