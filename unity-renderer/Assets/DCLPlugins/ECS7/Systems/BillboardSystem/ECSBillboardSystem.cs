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
            Vector3 cameraUp = cameraT.up;
            Vector3 cameraForward = cameraT.forward;

            var billboards = billboardComponent.Get();

            for (var i = 0; i < billboards.Count; i++)
            {
                uint billboardMode = (uint)billboards[i].value.model.GetBillboardMode();
                IDCLEntity entity = billboards[i].value.entity;
                Transform billboardT = entity.gameObject.transform;

                if (billboardMode == BILLBOARD_NONE)
                {
                    billboardT.forward = Vector3.back;
                    continue;
                }

                uint billboardModeXY = (billboardMode & BILLBOARD_XY);

                Vector3 billboardForward = billboardT.forward;
                Vector3 billboardPos = billboardT.position;

                Vector3 forward = billboardForward;
                Vector3 up = Vector3.up;

                // either or both X and Y are set
                if (billboardModeXY != 0)
                {
                    forward = cameraForward;

                    // only one of X and Y is set
                    if (billboardModeXY != BILLBOARD_XY)
                    {
                        if ((billboardMode & BILLBOARD_Y) != 0)
                        {
                            forward.y = billboardForward.y;
                        }

                        if ((billboardMode & BILLBOARD_X) != 0)
                        {
                            forward.x = billboardForward.x;
                        }

                        forward.Normalize();
                    }
                }

                // Z is set
                if ((billboardMode & BILLBOARD_Z) != 0)
                {
                    up = cameraUp;
                }

                billboardT.LookAt(billboardPos + forward, up);
            }
        }

        public static Quaternion GetXAxisRotation(Quaternion quaternion)
        {
            float a = Mathf.Sqrt((quaternion.w * quaternion.w) + (quaternion.x * quaternion.x));
            return new Quaternion(x: quaternion.x, y: 0, z: 0, w: quaternion.w / a);
        }

        public static Quaternion GetYAxisRotation(Quaternion quaternion)
        {
            float a = Mathf.Sqrt((quaternion.w * quaternion.w) + (quaternion.y * quaternion.y));
            return new Quaternion(x: 0, y: quaternion.y, z: 0, w: quaternion.w / a);
        }

        public static Quaternion GetZAxisRotation(Quaternion quaternion)
        {
            float a = Mathf.Sqrt((quaternion.w * quaternion.w) + (quaternion.z * quaternion.z));
            return new Quaternion(x: 0, y: 0, z: quaternion.z, w: quaternion.w / a);
        }
    }
}
