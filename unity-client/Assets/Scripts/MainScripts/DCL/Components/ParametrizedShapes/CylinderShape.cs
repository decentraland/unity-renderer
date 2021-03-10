using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using UnityEngine;

namespace DCL.Components
{
    public class CylinderShape : ParametrizedShape<CylinderShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
            public float radiusTop = 1f;
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

        public CylinderShape(IParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.CYLINDER_SHAPE;
        }

        public override Mesh GenerateGeometry()
        {
            var model = (Model)this.model;
            return PrimitiveMeshBuilder.BuildCylinder(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            if (currentMesh == null) return true;

            Model newCylinderModel = newModel as Model;
            var model = (Model)this.model;
            return  newCylinderModel.radius != model.radius
                    || newCylinderModel.radiusTop != model.radiusTop
                    || newCylinderModel.radiusBottom != model.radiusBottom
                    || newCylinderModel.segmentsHeight != model.segmentsHeight
                    || newCylinderModel.segmentsRadial != model.segmentsRadial
                    || newCylinderModel.openEnded != model.openEnded
                    || newCylinderModel.arc != model.arc;
        }
    }
}