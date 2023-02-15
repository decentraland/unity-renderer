using DCLServices.MapRendererV2.CoordsUtils;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.Input
{
    internal partial class MapInputController : IMapInputController
    {
        public event Action<Vector2Int> OnParcelHoverEnter;
        public event Action<Vector2Int> OnParcelHoverExit;
        public event Action<Vector2Int> OnParcelClicked;

        private readonly ICoordsUtils coordsUtils;

        private Vector2Int? currentHover = null;
        private Vector2Int? currentPointerDown = null;

        internal MapInputController(ICoordsUtils coordsUtils)
        {
            this.coordsUtils = coordsUtils;
        }

        public void Dispose() { }

        public void Hover(Vector2 worldpos)
        {
            var newHover = coordsUtils.PositionToCoordsInWorld(worldpos);

            if (newHover == currentHover)
                return;

            if (currentHover != null)
                OnParcelHoverExit?.Invoke(currentHover.Value);

            currentHover = newHover;

            if (currentHover != null)
                OnParcelHoverEnter?.Invoke(currentHover.Value);
        }

        public void PointerDown(Vector2 worldpos)
        {
            currentPointerDown = coordsUtils.PositionToCoordsInWorld(worldpos);
        }

        public void PointerUp(Vector2 worldpos)
        {
            if (currentPointerDown == null)
                return;

            var pointerDownParcel = currentPointerDown;
            var pointerUpParcel = coordsUtils.PositionToCoordsInWorld(worldpos);
            currentPointerDown = null;

            if (pointerDownParcel == pointerUpParcel)
                OnParcelClicked?.Invoke(pointerDownParcel.Value);
        }
    }
}
