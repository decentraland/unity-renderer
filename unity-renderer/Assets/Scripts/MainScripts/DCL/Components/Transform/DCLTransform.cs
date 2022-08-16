using System.Collections;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class DCLTransform : IEntityComponent, IOutOfSceneBoundariesHandler
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;

            public override BaseModel GetDataFromJSON(string json)
            {
                DCLTransformUtils.DecodeTransform(json, ref DCLTransform.model);
                return DCLTransform.model;
            }
        }
        
        public static Model model = new Model();

        public void Cleanup() { }

        public string componentName { get; } = "Transform";
        public IParcelScene scene { get; private set; }
        public IDCLEntity entity { get; private set; }
        public Transform GetTransform() => null;
        
        private const int MAX_TRANSFORM_VALUE = 10000;

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

            if (entity.OnTransformChange != null) // AvatarShape interpolation hack
            {
                entity.OnTransformChange.Invoke(DCLTransform.model);
            }
            else
            {
                entity.gameObject.transform.localPosition = DCLTransform.model.position;
                entity.gameObject.transform.localRotation = DCLTransform.model.rotation;
                entity.gameObject.transform.localScale = DCLTransform.model.scale;
                
                CapTransformGlobalValuesToMax(entity.gameObject.transform);
            }
        }

        public IEnumerator ApplyChanges(BaseModel model) { return null; }

        public void RaiseOnAppliedChanges() { }

        public bool IsValid() => true;
        public BaseModel GetModel() => DCLTransform.model;
        public int GetClassId() => (int) CLASS_ID_COMPONENT.TRANSFORM;
        public void UpdateOutOfBoundariesState(bool enable) { }
        
        public static void CapTransformGlobalValuesToMax(Transform transform)
        {
            bool positionOutsideBoundaries = transform.position.magnitude > MAX_TRANSFORM_VALUE;
            bool scaleOutsideBoundaries = transform.lossyScale.magnitude > MAX_TRANSFORM_VALUE;
            
            if (positionOutsideBoundaries || scaleOutsideBoundaries)
            {
                Vector3 newPosition = transform.position;
                if (positionOutsideBoundaries)
                {
                    if (Mathf.Abs(newPosition.x) > MAX_TRANSFORM_VALUE)
                        newPosition.x = MAX_TRANSFORM_VALUE * Mathf.Sign(newPosition.x);

                    if (Mathf.Abs(newPosition.y) > MAX_TRANSFORM_VALUE)
                        newPosition.y = MAX_TRANSFORM_VALUE * Mathf.Sign(newPosition.y);

                    if (Mathf.Abs(newPosition.z) > MAX_TRANSFORM_VALUE)
                        newPosition.z = MAX_TRANSFORM_VALUE * Mathf.Sign(newPosition.z);
                }
                
                Vector3 newScale = transform.lossyScale;
                if (scaleOutsideBoundaries)
                {
                    if (Mathf.Abs(newScale.x) > MAX_TRANSFORM_VALUE)
                        newScale.x = MAX_TRANSFORM_VALUE * Mathf.Sign(newScale.x);

                    if (Mathf.Abs(newScale.y) > MAX_TRANSFORM_VALUE)
                        newScale.y = MAX_TRANSFORM_VALUE * Mathf.Sign(newScale.y);

                    if (Mathf.Abs(newScale.z) > MAX_TRANSFORM_VALUE)
                        newScale.z = MAX_TRANSFORM_VALUE * Mathf.Sign(newScale.z);
                }
                
                SetTransformGlobalValues(transform, newPosition, transform.rotation, newScale, true);
            }
        }
        
        public static void SetTransformGlobalValues(Transform transform, Vector3 newPos, Quaternion newRot, Vector3 newScale, bool setScale = true)
        {
            transform.position = newPos;
            transform.rotation = newRot;

            if (setScale)
            {
                transform.localScale = Vector3.one;
                var m = transform.worldToLocalMatrix;

                m.SetColumn(0, new Vector4(m.GetColumn(0).magnitude, 0f));
                m.SetColumn(1, new Vector4(0f, m.GetColumn(1).magnitude));
                m.SetColumn(2, new Vector4(0f, 0f, m.GetColumn(2).magnitude));
                m.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

                transform.localScale = m.MultiplyPoint(newScale);
            }
        }
    }
}