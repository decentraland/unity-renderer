using System;
using System.Collections;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public class Billboard : BaseComponent
    {
        [Serializable]
        public class Model : BaseModel
        {
            public bool x = true;
            public bool y = true;
            public bool z = true;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }


        private const string COMPONENT_NAME = "billboard";
        private Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;

        internal Transform Tr { get; private set; }
        internal Transform EntityTransform { get; private set; }
        internal Vector3 LastPosition { get; set; }




        private void Awake() 
        {
            model = new Model();
            Tr = transform;

            Environment.i.serviceLocator.Get<IBillboardsController>().BillboardAdded(gameObject);
        }

        private void OnDestroy()
        {
            cameraPosition.OnChange -= CameraPositionChanged;

            Environment.i.serviceLocator.Get<IBillboardsController>().BillboardRemoved(gameObject);
        }

        public override string componentName => COMPONENT_NAME;

        public override int GetClassId()
        {
            return (int)CLASS_ID_COMPONENT.BILLBOARD;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            cameraPosition.OnChange -= CameraPositionChanged;
            cameraPosition.OnChange += CameraPositionChanged;

            Model model = (Model)newModel;

            ChangeOrientation();


            if (EntityTransform == null)
            {
                yield return new WaitUntil(() => entity.gameObject != null);
                EntityTransform = entity.gameObject.transform;
            }
        }

        new public Model GetModel() 
        { 
            return (Model)model; 
        }

        internal Vector3 GetLookAtVector()
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

        private void ChangeOrientation()
        {
            if (EntityTransform == null)
                return;

            Vector3 lookAtVector = GetLookAtVector();
            if (lookAtVector != Vector3.zero)
                EntityTransform.forward = lookAtVector;
        }

        private void CameraPositionChanged(Vector3 current, Vector3 previous) { ChangeOrientation(); }
    }
}