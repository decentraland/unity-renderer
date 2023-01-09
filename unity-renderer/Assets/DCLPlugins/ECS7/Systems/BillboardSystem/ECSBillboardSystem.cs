using System;
using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

namespace ECSSystems.BillboardSystem
{
    public class ECSBillboardSystem
    {
        private readonly ECSComponent<PBBillboard> billboardComponent;
        private readonly DataStore_Camera camera;

        public ECSBillboardSystem(ECSComponent<PBBillboard> billboards,  DataStore_Camera camera)
        {
            this.billboardComponent = billboards;
            this.camera = camera;
        }

        public void Update()
        {
            UnityEngine.Vector3 cameraPosition = camera.transform.Get().position;
            var billboards = billboardComponent.Get();

            for (var i = 0; i < billboards.Count; i++)
            {
                PBBillboard billboard = billboards[i].value.model;
                IDCLEntity entity = billboards[i].value.entity;
                UnityEngine.Vector3 lookAtVector = // billboard.OppositeDirection ?
                    // cameraPosition - entity.gameObject.transform.position :
                    entity.gameObject.transform.position - cameraPosition;

                if (billboard.BillboardMode == BillboardMode.BmY)
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
