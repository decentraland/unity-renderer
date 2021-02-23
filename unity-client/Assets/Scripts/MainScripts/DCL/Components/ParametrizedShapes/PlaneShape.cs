using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class PlaneShape : ParametrizedShape<PlaneShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
            public float[] uvs;
            public float width = 1f; // Plane
            public float height = 1f; // Plane
        }

        public PlaneShape(IParcelScene scene) : base(scene)
        {
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.PLANE_SHAPE;
        }

        public override Mesh GenerateGeometry()
        {
            Mesh mesh = PrimitiveMeshBuilder.BuildPlane(1f);

            if (model.uvs != null && model.uvs.Length > 0)
            {
                mesh.uv = Utils.FloatArrayToV2List(model.uvs);
            }

            return mesh;
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            if (currentMesh == null)
                return true;

            Model newPlaneModel = newModel as Model;

            if (newPlaneModel.uvs != null && model.uvs != null)
            {
                if (newPlaneModel.uvs.Length != model.uvs.Length)
                    return true;

                for (int i = 0; i < newPlaneModel.uvs.Length; i++)
                {
                    if (newPlaneModel.uvs[i] != model.uvs[i])
                        return true;
                }
            }
            else
            {
                if (newPlaneModel.uvs != model.uvs)
                    return true;
            }

            return newPlaneModel.width != model.width || newPlaneModel.height != model.height;
        }
    }
}