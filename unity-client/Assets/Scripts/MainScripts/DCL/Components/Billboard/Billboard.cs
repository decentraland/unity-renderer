using DCL.Components;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using DCL.Models;

namespace DCL
{
    public class Billboard : BaseComponent
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public bool x = true;
            public bool y = true;
            public bool z = true;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        Transform entityTransform;
        Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
        Vector3 lastPosition;

        private void Awake()
        {
            model = new Model();
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            cameraPosition.OnChange -= CameraPositionChanged;
            cameraPosition.OnChange += CameraPositionChanged;

            Model model = (Model)newModel;

            ChangeOrientation();


            if (entityTransform == null)
            {
                yield return new WaitUntil(() => entity.gameObject != null);
                entityTransform = entity.gameObject.transform;
            }
        }

        new public Model GetModel()
        {
            return (Model)model;
        }

        public void OnDestroy()
        {
            cameraPosition.OnChange -= CameraPositionChanged;
        }

        // This runs on LateUpdate() instead of Update() to be applied AFTER the transform was moved by the transform component
        public void LateUpdate()
        {
            //NOTE(Brian): This fixes #757 (https://github.com/decentraland/unity-client/issues/757)
            //             We must find a more performant way to handle this, until that time, this is the approach.

            if (entityTransform == null) return;
            if (transform.position == lastPosition) return;

            lastPosition = transform.position;

            ChangeOrientation();
        }

        Vector3 GetLookAtVector()
        {
            bool hasTextShape = entity.components.ContainsKey(Models.CLASS_ID_COMPONENT.TEXT_SHAPE);
            Vector3 lookAtDir = hasTextShape ? (entityTransform.position - cameraPosition) : (cameraPosition - entityTransform.position);

            Model model = (Model) this.model;
            // Note (Zak): This check is here to avoid normalizing twice if not needed
            if (!(model.x && model.y && model.z))
            {
                lookAtDir.Normalize();

                // Note (Zak): Model x,y,z are axis that we want to enable/disable
                // while lookAtDir x,y,z are the components of the look-at vector
                if (!model.x || model.z)
                    lookAtDir.y = entityTransform.forward.y;
                if (!model.y)
                    lookAtDir.x = entityTransform.forward.x;
            }

            return lookAtDir.normalized;
        }

        void ChangeOrientation()
        {
            if (entityTransform == null)
                return;
            Vector3 lookAtVector = GetLookAtVector();
            if(lookAtVector != Vector3.zero)
                entityTransform.forward = lookAtVector;
        }

        private void CameraPositionChanged(Vector3 current, Vector3 previous)
        {
            ChangeOrientation();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.BILLBOARD;
        }
    }
}
