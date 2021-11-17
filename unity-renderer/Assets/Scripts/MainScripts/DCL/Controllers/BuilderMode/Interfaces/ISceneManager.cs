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
    }
}