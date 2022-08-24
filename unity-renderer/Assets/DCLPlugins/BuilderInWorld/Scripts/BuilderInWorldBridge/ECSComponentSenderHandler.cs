using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Builder
{
    // TODO: When we have the new ECS, we will implement this functionality
    public class ECSComponentSenderHandler : IComponentSendHandler
    {
        public void SendNewEntityToKernel(string sceneId, long entityId, ProtocolV2.ComponentPayload[] componentsPayload) { throw new System.NotImplementedException(); }
        public void ChangeEntityComponent(ProtocolV2.EntitySingleComponentPayload payload, IParcelScene scene) { throw new System.NotImplementedException(); }
        public void EntityTransformReport(IDCLEntity entity, IParcelScene scene) { throw new System.NotImplementedException(); }
        public void RemoveEntityOnKernel(long entityId, IParcelScene scene) { throw new System.NotImplementedException(); }
    }
}