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
        
        private Transform entityTransform;
        private Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
        private UnityEngine.Vector3 lastPosition;

        private IDCLEntity entity;
        private IParcelScene scene;
        private PBBillboard model;
        
        public BillboardComponentHandler(IUpdateEventHandler updateEventHandler)
        {
            this.updateEventHandler = updateEventHandler;
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            this.entity = entity;
            this.scene = scene;

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
            if (entityTransform.position == lastPosition)
                return;

            lastPosition = entityTransform.position;

            ChangeOrientation();
        }

        /// <summary>
        /// We use the model axis as a lock values. This means, that if the model.X comes true, we lock the X axis to look at camera
        /// </summary>
        /// <returns></returns>
        private UnityEngine.Vector3 GetLookAtVector()
        {
            UnityEngine.Vector3 lookAtDir =(cameraPosition - entityTransform.position);
            
            // Note (Zak): This check is here to avoid normalizing twice if not needed
            if (!(model.X && model.Y && model.Z))
            {
                lookAtDir.Normalize();

                // Note (Zak): Model x,y,z are axis that we want to enable/disable
                // while lookAtDir x,y,z are the components of the look-at vector
                if (!model.X || model.Z)
                    lookAtDir.y = entityTransform.forward.y;
                if (!model.Y)
                    lookAtDir.x = entityTransform.forward.x;
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