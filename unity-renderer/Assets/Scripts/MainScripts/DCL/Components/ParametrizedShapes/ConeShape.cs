using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class ConeShape : ParametrizedShape<ConeShape.Model>
    {
        [System.Serializable]
        public new class Model : BaseShape.Model
        {
            public float radiusTop;
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
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.ConeShape)
                    return Utils.SafeUnimplemented<ConeShape, Model>(expected: ComponentBodyPayload.PayloadOneofCase.ConeShape, actual: pbModel.PayloadCase);
                
                var pb = new Model();
                if (pbModel.ConeShape.HasArc) pb.arc = pbModel.ConeShape.Arc;
                if (pbModel.ConeShape.HasRadius) pb.radius = pbModel.ConeShape.Radius;
                if (pbModel.ConeShape.HasOpenEnded) pb.openEnded = pbModel.ConeShape.OpenEnded;
                if (pbModel.ConeShape.HasRadiusBottom) pb.radiusBottom = pbModel.ConeShape.RadiusBottom;
                if (pbModel.ConeShape.HasRadiusTop) pb.radiusTop = pbModel.ConeShape.RadiusTop;
                if (pbModel.ConeShape.HasSegmentsHeight) pb.segmentsHeight = pbModel.ConeShape.SegmentsHeight;
                if (pbModel.ConeShape.HasSegmentsRadial) pb.segmentsRadial = pbModel.ConeShape.SegmentsRadial;
                if (pbModel.ConeShape.HasVisible) pb.visible = pbModel.ConeShape.Visible;
                if (pbModel.ConeShape.HasWithCollisions) pb.withCollisions = pbModel.ConeShape.WithCollisions;
                if (pbModel.ConeShape.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.ConeShape.IsPointerBlocker;
                
                return pb;
            }
        }

        public ConeShape()
        {
            model = new Model();
        }

        public override int GetClassId() =>
            (int) CLASS_ID.CONE_SHAPE;

        public override Mesh GenerateGeometry()
        {
            var model = (Model) this.model;
            return PrimitiveMeshBuilder.BuildCone(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            if (currentMesh == null)
                return true;

            Model newConeModel = newModel as Model;
            var model = (Model) this.model;
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
