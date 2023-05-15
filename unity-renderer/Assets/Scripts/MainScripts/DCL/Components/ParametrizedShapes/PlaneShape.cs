using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;
using System.Linq;

namespace DCL.Components
{
    public class PlaneShape : ParametrizedShape<PlaneShape.Model>
    {
        [System.Serializable]
        public new class Model : BaseShape.Model
        {
            public float[] uvs;
            public float width = 1f; // Plane
            public float height = 1f; // Plane

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.PlaneShape)
                    return Utils.SafeUnimplemented<PlaneShape, Model>(expected: ComponentBodyPayload.PayloadOneofCase.PlaneShape, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.PlaneShape.Uvs is { Count: > 0 }) pb.uvs = pbModel.PlaneShape.Uvs.ToArray();
                if (pbModel.PlaneShape.HasHeight) pb.height = pbModel.PlaneShape.Height;
                if (pbModel.PlaneShape.HasWidth) pb.width = pbModel.PlaneShape.Width;
                if (pbModel.PlaneShape.HasVisible) pb.visible = pbModel.PlaneShape.Visible;
                if (pbModel.PlaneShape.HasWithCollisions) pb.withCollisions = pbModel.PlaneShape.WithCollisions;
                if (pbModel.PlaneShape.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.PlaneShape.IsPointerBlocker;
                
                return pb;
            }
        }

        public PlaneShape() { model = new Model(); }

        public override int GetClassId() { return (int) CLASS_ID.PLANE_SHAPE; }

        public override Mesh GenerateGeometry()
        {
            var model = (Model) this.model;

            Mesh mesh = PrimitiveMeshBuilder.BuildPlane(1f);
            if (model.uvs != null && model.uvs.Length > 0)
            {
                mesh.uv = Utils.FloatArrayToV2List(model.uvs);
            }

            return mesh;
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model previousModel)
        {
            if (currentMesh == null)
                return true;

            PlaneShape.Model newPlaneModel = (PlaneShape.Model) this.model;
            PlaneShape.Model oldPlaneModel = (PlaneShape.Model) previousModel;

            if (newPlaneModel.uvs != null && oldPlaneModel.uvs != null)
            {
                if (newPlaneModel.uvs.Length != oldPlaneModel.uvs.Length)
                    return true;

                for (int i = 0; i < newPlaneModel.uvs.Length; i++)
                {
                    if (newPlaneModel.uvs[i] != oldPlaneModel.uvs[i])
                        return true;
                }
            }
            else
            {
                if (newPlaneModel.uvs != oldPlaneModel.uvs)
                    return true;
            }

            return newPlaneModel.width != oldPlaneModel.width || newPlaneModel.height != oldPlaneModel.height;
        }
    }
}
