using System;
using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;

namespace ECSSystems.BillboardSystem
{
    public class ECSBillboardSystem : IDisposable
    {
        
        private class State
        {
            public SystemsContext context;
            public DataStore_Camera camera;
        }

        private readonly State state;

        public ECSBillboardSystem(SystemsContext context, DataStore_Camera camera)
        {
            state = new State()
            {
                context = context,
                camera = camera
            };

        }

        public void Dispose()
        {
        }

        public void Update()
        {
            UnityEngine.Vector3 cameraPosition = state.camera.transform.Get().position;
            UnityEngine.Vector3 lookAtVector;
            var billboards = state.context.billboards.Get();

            for (var i = 0; i < billboards.Count; i++)
            {
                PBBillboard billboard = billboards[i].value.model;
                IDCLEntity entity = billboards[i].value.entity;
                IParcelScene scene = billboards[i].value.scene;
                UnityEngine.Vector3 transformPosition = state.context.transforms.Get(scene, entity).model.position;
                
                if (billboard.OppositeDirection)
                {
                    lookAtVector = new UnityEngine.Vector3(cameraPosition.x - transformPosition.x, cameraPosition.y - transformPosition.y, cameraPosition.z - transformPosition.z);
                }
                else
                {
                    lookAtVector = new UnityEngine.Vector3(transformPosition.x - cameraPosition.x, transformPosition.y - cameraPosition.y, transformPosition.z - cameraPosition.z);
                }
                
                if (billboard.BillboardMode == BillboardMode.BmYAxe)
                {
                    lookAtVector.Normalize();
                    lookAtVector.y = entity.gameObject.transform.forward.y;
                    
                }
                
                if (lookAtVector != UnityEngine.Vector3.zero)
                    entity.gameObject.transform.forward = lookAtVector.normalized;
            }
        }
    }
}