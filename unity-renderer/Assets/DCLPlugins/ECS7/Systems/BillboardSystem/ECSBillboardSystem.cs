using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace ECSSystems.BillboardSystem
{
    public class ECSBillboardSystem
    {
        private readonly ECSComponent<PBBillboard> billboardComponent;
        private readonly DataStore_Camera camera;

        public ECSBillboardSystem(ECSComponent<PBBillboard> billboards, DataStore_Camera camera)
        {
            this.billboardComponent = billboards;
            this.camera = camera;
        }

        public void Update()
        {
            const uint BILLBOARD_NONE = (uint)BillboardMode.BmNone;
            const uint BILLBOARD_X = (uint)BillboardMode.BmX;
            const uint BILLBOARD_Y = (uint)BillboardMode.BmY;
            const uint BILLBOARD_Z = (uint)BillboardMode.BmZ;
            const uint BILLBOARD_XY = BILLBOARD_X | BILLBOARD_Y;

            Transform cameraT = camera.transform.Get();
            Vector3 cameraPos = cameraT.position;
            Quaternion cameraRotationAxisZ = Quaternion.Euler(0, 0, cameraT.rotation.eulerAngles.z);

            var billboards = billboardComponent.Get();

            for (var i = 0; i < billboards.Count; i++)
            {
                uint billboardMode = (uint)billboards[i].value.model.GetBillboardMode();
                IDCLEntity entity = billboards[i].value.entity;
                Transform billboardT = entity.gameObject.transform;

                if (billboardMode == BILLBOARD_NONE)
                {
                    continue;
                }

                Vector3 billboardForward = billboardT.forward;
                Vector3 billboardPos = billboardT.position;

                Vector3 forward = billboardForward;

                // either or both X and Y are set
                if ((billboardMode & BILLBOARD_XY) != 0)
                {
                    forward = billboardPos - cameraPos;

                    if ((billboardMode & BILLBOARD_Y) == 0) forward.x = 0;
                    if ((billboardMode & BILLBOARD_X) == 0) forward.y = 0;

                    forward.Normalize();
                }

                Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

                // apply Z axis rotation
                if ((billboardMode & BILLBOARD_Z) != 0)
                {
                    rotation *= cameraRotationAxisZ;
                }

                billboardT.rotation = rotation;
            }
        }
    }
}
