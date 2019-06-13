using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class BoxShape : BaseParametrizedShape<BoxShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
        }

        public BoxShape(ParcelScene scene) : base(scene) { }

        public static Mesh cubeMesh = null;
        private static int cubeMeshUses = 0;

        public override Mesh GenerateGeometry()
        {
            if (cubeMesh == null)
            {
                cubeMesh = PrimitiveMeshBuilder.BuildCube(1f);
            }
            cubeMeshUses++;

            return cubeMesh;
        }

        protected override void DestroyGeometry()
        {
            cubeMeshUses--;
            if (cubeMeshUses == 0)
            {
                GameObject.Destroy(cubeMesh);
            }
        }
    }
}