using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class BoxShape : ParametrizedShape<BoxShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        public BoxShape(IParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public static Mesh cubeMesh = null;
        private static int cubeMeshRefCount = 0;

        public override int GetClassId()
        {
            return (int) CLASS_ID.BOX_SHAPE;
        }

        public override Mesh GenerateGeometry()
        {
            if (cubeMesh == null)
                cubeMesh = PrimitiveMeshBuilder.BuildCube(1f);

            cubeMeshRefCount++;
            return cubeMesh;
        }

        protected override void DestroyGeometry()
        {
            cubeMeshRefCount--;

            if (cubeMeshRefCount == 0)
            {
                GameObject.Destroy(cubeMesh);
                cubeMesh = null;
            }
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            return currentMesh == null;
        }
    }
}