using DCLServices.MapRendererV2.ConsumerUtils;
using System;
using UnityEngine;

namespace DCL
{
    [Serializable]
    public class NavmapRendererConfiguration
    {
        [field: SerializeField]
        public MapRenderImage RenderImage { get; private set; }

        [field: SerializeField]
        public PixelPerfectMapRendererTextureProvider PixelPerfectMapRendererTextureProvider { get; private set; }

        [field: SerializeField]
        public MapCameraDragBehavior.MapCameraDragBehaviorData MapCameraDragBehaviorData { get; private set; }
    }
}
