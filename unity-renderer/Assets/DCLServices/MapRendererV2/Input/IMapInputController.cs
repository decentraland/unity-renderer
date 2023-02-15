using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.Input
{
    internal interface IMapInputController : IDisposable
    {
        event Action<Vector2Int> OnParcelHoverEnter;
        event Action<Vector2Int> OnParcelHoverExit;
        event Action<Vector2Int> OnParcelClicked;

        /// <summary>
        /// Report hover at the provided position
        /// </summary>
        /// <param name="worldpos">position relative to the original render texture</param>
        void Hover(Vector2 worldpos);

        /// <summary>
        /// Report pointer down event at the provided position
        /// </summary>
        /// <param name="worldpos">position relative to the original render texture</param>
        void PointerDown(Vector2 worldpos);

        /// <summary>
        /// Report pointer up event at the provided position
        /// </summary>
        /// <param name="worldpos"></param>
        void PointerUp(Vector2 worldpos);
    }
}
