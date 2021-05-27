using System.Collections;
using System.Net.Configuration;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class DCLTransform : IEntityComponent
    {
        const float VECTOR3_MEMBER_CAP = 1000000; // Value measured when genesis plaza glitch triggered a physics engine breakdown

        [System.Serializable]
        public class Model : BaseModel
        {
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;

            public override BaseModel GetDataFromJSON(string json)
            {
                MessageDecoder.DecodeTransform(json, ref DCLTransform.model);
                return DCLTransform.model;
            }
        }

        public static Model model = new Model();

        public void Cleanup() { }

        public string componentName { get; } = "Transform";
        public IParcelScene scene { get; private set; }
        public IDCLEntity entity { get; private set; }
        public Transform GetTransform() => null;

        public void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            this.entity = entity;
        }

        public void UpdateFromJSON(string json)
        {
            model.GetDataFromJSON(json);
            UpdateFromModel(model);
        }

        public void UpdateFromModel(BaseModel model)
        {
            DCLTransform.model = model as Model;

            if (entity.OnTransformChange != null)
            {
                entity.OnTransformChange.Invoke(DCLTransform.model);
            }
            else
            {
                // If we don't cap these vectors, some scenes may trigger a physics breakdown when messaging enormous values
                DCLTransform.model.position = CapVector3(DCLTransform.model.position);
                DCLTransform.model.scale = CapVector3(DCLTransform.model.scale);

                entity.gameObject.transform.localPosition = DCLTransform.model.position;
                entity.gameObject.transform.localRotation = DCLTransform.model.rotation;
                entity.gameObject.transform.localScale = DCLTransform.model.scale;

                DCL.Environment.i.world.sceneBoundsChecker?.AddEntityToBeChecked(entity);
            }
        }

        private Vector3 CapVector3(Vector3 targetVector)
        {
            if (Mathf.Abs(targetVector.x) > VECTOR3_MEMBER_CAP)
                targetVector.x = VECTOR3_MEMBER_CAP * Mathf.Sign(targetVector.x);

            if (Mathf.Abs(targetVector.y) > VECTOR3_MEMBER_CAP)
                targetVector.y = VECTOR3_MEMBER_CAP * Mathf.Sign(targetVector.y);

            if (Mathf.Abs(targetVector.z) > VECTOR3_MEMBER_CAP)
                targetVector.z = VECTOR3_MEMBER_CAP * Mathf.Sign(targetVector.z);

            return targetVector;
        }

        public IEnumerator ApplyChanges(BaseModel model) { return null; }

        public void RaiseOnAppliedChanges() { }

        public bool IsValid() => true;
        public BaseModel GetModel() => DCLTransform.model;
        public int GetClassId() => (int) CLASS_ID_COMPONENT.TRANSFORM;
    }
}