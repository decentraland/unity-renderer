using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using UnityEngine;

namespace DCL.Components
{
    public class ConeShape : ParametrizedShape<ConeShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
            public float radiusTop = 0f;
            public float radiusBottom = 1f;
            public float segmentsHeight = 1f;
            public float segmentsRadial = 36f;
            public bool openEnded = false;
            public float? radius;
            public float arc = 360f;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }
        
        public ConeShape(IParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.CONE_SHAPE;
        }

        public override Mesh GenerateGeometry()
        {
            var model = (Model)this.model;
            return PrimitiveMeshBuilder.BuildCone(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            if (currentMesh == null)
                return true;

            Model newConeModel = newModel as Model;
            var model = (Model)this.model;
            return newConeModel.radius != model.radius
                   || newConeModel.radiusTop != model.radiusTop
                   || newConeModel.radiusBottom != model.radiusBottom
                   || newConeModel.segmentsHeight != model.segmentsHeight
                   || newConeModel.segmentsRadial != model.segmentsRadial
                   || newConeModel.openEnded != model.openEnded
                   || newConeModel.arc != model.arc;
        }
    }
}
