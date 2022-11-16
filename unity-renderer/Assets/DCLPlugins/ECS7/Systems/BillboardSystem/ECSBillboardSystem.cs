using System;
using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

namespace ECSSystems.BillboardSystem
{
    public class ECSBillboardSystem : IDisposable
    {
        
        private class State
        {
            public ECSComponent<PBBillboard> billboards;
            public DataStore_Camera camera;
        }

        private readonly State state;

        public ECSBillboardSystem(ECSComponent<PBBillboard> billboards,  DataStore_Camera camera)
        {
            state = new State()
            {
                billboards = billboards,
                camera = camera
            };

        }

        public void Dispose()
        {
        }

        public void Update()
        {
            UnityEngine.Vector3 cameraPosition = state.camera.transform.Get().position;
            var billboards = state.billboards.Get();

            for (var i = 0; i < billboards.Count; i++)
            {
                PBBillboard billboard = billboards[i].value.model;
                IDCLEntity entity = billboards[i].value.entity;
                UnityEngine.Vector3 lookAtVector = billboard.OppositeDirection ? 
                    cameraPosition - entity.gameObject.transform.position : 
                    entity.gameObject.transform.position - cameraPosition; 

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