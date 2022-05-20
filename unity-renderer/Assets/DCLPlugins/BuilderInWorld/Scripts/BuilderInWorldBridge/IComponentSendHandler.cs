using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using static ProtocolV2;

namespace DCL.Builder
{
    public interface IComponentSendHandler
    {
        /// <summary>
        /// Send the new entity to the kernel
        /// </summary>
        /// <param name="sceneId"></param>
        /// <param name="entityId"></param>
        /// <param name="componentsPayload"></param>
        void SendNewEntityToKernel(string sceneId, long entityId, ComponentPayload[] componentsPayload);
        
        /// <summary>
        /// Update an entity component with the new model
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="scene"></param>
        void ChangeEntityComponent(EntitySingleComponentPayload payload, IParcelScene scene);
        
        /// <summary>
        /// Report the transform component for an entity to kernel
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="scene"></param>
        void EntityTransformReport(IDCLEntity entity, IParcelScene scene);
        
        /// <summary>
        /// Remove the entity from the kernel
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="scene"></param>
        void RemoveEntityOnKernel(long entityId, IParcelScene scene);
    }
}
