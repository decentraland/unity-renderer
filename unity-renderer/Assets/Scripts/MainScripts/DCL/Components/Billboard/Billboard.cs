using System;
using System.Collections;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

namespace DCL
{
    public class Billboard : BaseComponent, IBillboard
    {
        [Serializable]
        public class Model : BaseModel
        {
            public bool x = true;
            public bool y = true;
            public bool z = true;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel) =>
                pbModel.PayloadCase == ComponentBodyPayload.PayloadOneofCase.Billboard
                    ? new Model
                    {
                        x = pbModel.Billboard.X,
                        y = pbModel.Billboard.Y,
                        z = pbModel.Billboard.Z,
                    }
                    : Utils.SafeUnimplemented<Billboard, Model>(expected: ComponentBodyPayload.PayloadOneofCase.Billboard, actual: pbModel.PayloadCase);
        }

        private const string COMPONENT_NAME = "billboard";

        public Transform Tr { get; set; }
        public Transform EntityTransform { get; set; }
        public Vector3 LastPosition { get; set; }


        public new Model GetModel() => (Model)model;

        public override string componentName => COMPONENT_NAME;


        public override int GetClassId()
        {
            return (int)CLASS_ID_COMPONENT.BILLBOARD;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            if (EntityTransform == null)
            {
                yield return new WaitUntil(() => entity.gameObject != null);
                EntityTransform = entity.gameObject.transform;
            }
        }


        public Vector3 GetLookAtVector(Vector3 cameraPosition)
        {
            bool hasTextShape = scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.TEXT_SHAPE);
            Vector3 lookAtDir = hasTextShape ? (EntityTransform.position - cameraPosition) : (cameraPosition - EntityTransform.position);

            Model model = (Model) this.model;
            // Note (Zak): This check is here to avoid normalizing twice if not needed
            if (!(model.x && model.y && model.z))
            {
                lookAtDir.Normalize();

                // Note (Zak): Model x,y,z are axis that we want to enable/disable
                // while lookAtDir x,y,z are the components of the look-at vector
                if (!model.x || model.z)
                    lookAtDir.y = EntityTransform.forward.y;
                if (!model.y)
                    lookAtDir.x = EntityTransform.forward.x;
            }

            return lookAtDir.normalized;
        }


        private void Awake()
        {
            model = new Model();
            Tr = transform;
            LastPosition = Vector3.up * float.MaxValue;

            IBillboardsController controller = Environment.i.serviceLocator.Get<IBillboardsController>();
            controller.BillboardAdded(this);
        }
    }
}
