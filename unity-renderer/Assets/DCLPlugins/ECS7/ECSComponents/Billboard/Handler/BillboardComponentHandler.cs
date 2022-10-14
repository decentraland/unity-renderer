using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class BillboardComponentHandler : IECSComponentHandler<PBBillboard>
    {
        private readonly IUpdateEventHandler updateEventHandler;
        private readonly DataStore_Player playerDataStore;
        
        private Transform entityTransform;
        private Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
        private UnityEngine.Vector3 lastPosition;

        private PBBillboard model;
        
        public BillboardComponentHandler(IUpdateEventHandler updateEventHandler)
        {
            this.updateEventHandler = updateEventHandler;
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            // The billboard will rotate the entity transform toward the camera
            entityTransform = entity.gameObject.transform;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBBillboard model)
        {
            this.model = model;
       
            ChangeOrientation();
        }
        
        // This runs on LateUpdate() instead of Update() to be applied AFTER the transform was moved by the transform component
        private void LateUpdate()
        {
            //NOTE(Brian): This fixes #757 (https://github.com/decentraland/unity-client/issues/757)
            //             We must find a more performant way to handle this, until that time, this is the approach.

            if (entityTransform == null)
                return;
       
            if (cameraPosition == lastPosition)
                return;
            
            lastPosition = cameraPosition;

            ChangeOrientation();
        }

        /// <summary>
        /// We use the model axis as a lock values. This means, that if the model.X comes true, we lock the X axis to look at camera
        /// </summary>
        /// <returns></returns>
        private UnityEngine.Vector3 GetLookAtVector()
        {
            UnityEngine.Vector3 lookAtDir = model.OppositeDirection ? (cameraPosition - entityTransform.position) : (entityTransform.position - cameraPosition);
            
            if (model.BillboardMode == BillboardMode.BmYAxe)
            {
                lookAtDir.Normalize();
                lookAtDir.y = entityTransform.forward.y;
            }

            return lookAtDir.normalized;
        }

        private void ChangeOrientation()
        {
            if (entityTransform == null || model == null)
                return;
            UnityEngine.Vector3 lookAtVector = GetLookAtVector();
            if (lookAtVector != UnityEngine.Vector3.zero)
                entityTransform.forward = lookAtVector;
        }
    }
}