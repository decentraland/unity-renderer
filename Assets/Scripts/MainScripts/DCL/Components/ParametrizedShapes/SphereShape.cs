using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class SphereShape : ParametrizedShape<SphereShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
        }

        public SphereShape(ParcelScene scene) : base(scene)
        {
        }

        public override int GetClassId()
        {
            return (int)CLASS_ID.SPHERE_SHAPE;
        }

        public static Mesh mesh = null;
        private static int meshUses = 0;

        public override Mesh GenerateGeometry()
        {
            if (mesh == null)
            {
                mesh = PrimitiveMeshBuilder.BuildSphere(1f);
            }
            meshUses++;

            return mesh;
        }

        protected override void DestroyGeometry()
        {
            meshUses--;
            if (meshUses == 0)
            {
                GameObject.Destroy(mesh);
            }
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            return currentMesh == null;
        }
    }
}