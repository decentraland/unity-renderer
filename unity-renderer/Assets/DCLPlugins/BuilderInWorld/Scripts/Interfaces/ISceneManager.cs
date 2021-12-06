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
    }
}