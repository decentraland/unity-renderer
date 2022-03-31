using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublisher
    {
        /// <summary>
        /// Released each time that the publish is finished
        /// </summary>
        event Action<bool> OnPublishFinish;

        /// <summary>
        /// This will Init the publisher
        /// </summary>
        /// <param name="context"></param>
        void Initialize(IContext context);
        
        /// <summary>
        /// This will dispose the publisher
        /// </summary>
        void Dispose();

        /// <summary>
        /// This will start the publish flow for the given scene.
        /// This handle all possible situations of the scene
        /// </summary>
        /// <param name="scene"></param>
        void StartPublish(IBuilderScene scene);

        /// <summary>
        /// This will publish an empty scene in 
        /// </summary>
        /// <param name="scene"></param>
        void Unpublish(Vector2Int coords, Vector2Int size);
    }
}