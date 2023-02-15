using UnityEngine;

namespace DCLServices.MapRendererV2.Input
{
    internal partial class MapInputController
    {
        public Vector2Int? CurrentHover
        {
            get => currentHover;
            set => currentHover = value;
        }

        public Vector2Int? CurrentPointerDown
        {
            get => currentPointerDown;
            set => currentPointerDown = value;
        }
    }
}
