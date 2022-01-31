using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublisher
    {
        void Initialize(IContext context);
        void Dispose();

        /// <summary>
        /// This will start the publish flow for the given scene.
        /// This handle all possible situations of the scene
        /// </summary>
        /// <param name="scene"></param>
        void StartPublish(IBuilderScene scene);
    }
}