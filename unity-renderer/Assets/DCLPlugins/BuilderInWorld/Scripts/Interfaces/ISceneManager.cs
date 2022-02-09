using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface ISceneManager
    {
        /// <summary>
        /// Initialize the system
        /// </summary>
        /// <param name="context"></param>
        void Initialize(IContext context);

        /// <summary>
        /// Dispose the system
        /// </summary>
        void Dispose();

        /// <summary>
        /// Update call from unity
        /// </summary>
        void Update();

        /// <summary>
        /// This start the flow of the editor from a manifest 
        /// </summary>
        /// <param name="manifest"></param>
        void StartFlowFromProject(Manifest.Manifest manifest);

        /// <summary>
        /// This will start the flow to edit a land where you are currently
        /// </summary>
        /// <param name="coords"></param>
        void StartFlowFromLandCoords(Vector2Int coords);

        /// <summary>
        /// This will show the loading screen of the builder with 0%
        /// </summary>
        void ShowBuilderLoading();

        /// <summary>
        /// Hide the builder loading
        /// </summary>
        void HideBuilderLoading();
    }
}