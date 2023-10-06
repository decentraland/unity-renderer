﻿using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PlayerMarker
{
    internal interface IPlayerMarker : IMapRendererMarker, IDisposable
    {
        void SetPosition(Vector3 position);

        void SetRotation(Quaternion rot);

        void SetActive(bool active);

        void SetZoom(float zoom);

        void ResetToBaseScale();
    }
}
