using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class CylinderShape : ParametrizedShape<CylinderShape.Model>
    {
        [System.Serializable]
        public new class Model : BaseShape.Model
        {
            public float radiusTop = 1f;
            public float radiusBottom = 1f;
            public float segmentsHeight = 1f;
            public float segmentsRadial = 36f;
            public bool openEnded;
            public float? radius;
            public float arc = 360f;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.CylinderShape)
                    return Utils.SafeUnimplemented<CylinderShape, Model>(expected: ComponentBodyPayload.PayloadOneofCase.CylinderShape, actual: pbModel.PayloadCase);
                
                var pb = new Model();
                if (pbModel.CylinderShape.HasArc) pb.arc = pbModel.CylinderShape.Arc;
                if (pbModel.CylinderShape.HasRadius) pb.radius = pbModel.CylinderShape.Radius;
                if (pbModel.CylinderShape.HasOpenEnded) pb.openEnded = pbModel.CylinderShape.OpenEnded;
                if (pbModel.CylinderShape.HasRadiusBottom) pb.radiusBottom = pbModel.CylinderShape.RadiusBottom;
                if (pbModel.CylinderShape.HasRadiusTop) pb.radiusTop = pbModel.CylinderShape.RadiusTop;
                if (pbModel.CylinderShape.HasSegmentsHeight) pb.segmentsHeight = pbModel.CylinderShape.SegmentsHeight;
                if (pbModel.CylinderShape.HasSegmentsRadial) pb.segmentsRadial = pbModel.CylinderShape.SegmentsRadial;
                if (pbModel.CylinderShape.HasVisible) pb.visible = pbModel.CylinderShape.Visible;
                if (pbModel.CylinderShape.HasWithCollisions) pb.withCollisions = pbModel.CylinderShape.WithCollisions;
                if (pbModel.CylinderShape.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.CylinderShape.IsPointerBlocker;
                
                return pb;
            }
        }

        public CylinderShape()
        {
            model = new Model();
        }

        public override int GetClassId() =>
            (int) CLASS_ID.CYLINDER_SHAPE;

        public override Mesh GenerateGeometry()
        {
            var model = (Model) this.model;
            return PrimitiveMeshBuilder.BuildCylinder(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            if (currentMesh == null)
                return true;

            Model newCylinderModel = newModel as Model;
            var model = (Model) this.model;
            return newCylinderModel.radius != model.radius
                   || newCylinderModel.radiusTop != model.radiusTop
                   || newCylinderModel.radiusBottom != model.radiusBottom
                   || newCylinderModel.segmentsHeight != model.segmentsHeight
                   || newCylinderModel.segmentsRadial != model.segmentsRadial
                   || newCylinderModel.openEnded != model.openEnded
                   || newCylinderModel.arc != model.arc;
        }
    }
}
