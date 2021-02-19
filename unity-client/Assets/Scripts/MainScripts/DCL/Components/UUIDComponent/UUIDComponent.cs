using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class UUIDComponent<ModelType> : UUIDComponent where ModelType : UUIDComponent.Model, new()
    {
        new public ModelType model
        {
            get { return base.model as ModelType; }
            set { base.model = value; }
        }
    }

    public class UUIDComponent : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            public string type;
            public string uuid;
        }

        public Model model = new Model();

        public virtual void Setup(ParcelScene scene, DecentralandEntity entity, UUIDComponent.Model model)
        {
        }

        public void RemoveFromEntity(DecentralandEntity entity, string type)
        {
            switch (type)
            {
                case OnClick.NAME:
                    RemoveComponent<OnClick>(entity);
                    break;
                case OnPointerDown.NAME:
                    RemoveComponent<OnPointerDown>(entity);
                    break;
                case OnPointerUp.NAME:
                    RemoveComponent<OnPointerUp>(entity);
                    break;
            }

            Debug.LogWarning($"Cannot remove UUIDComponent of type '{type}'.");
        }

        protected virtual void RemoveComponent<T>(DecentralandEntity entity) where T : UUIDComponent
        {
            var currentComponent = entity.gameObject.GetComponent<T>();

            if (currentComponent != null)
            {
#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(currentComponent);
#else
                UnityEngine.Object.Destroy(currentComponent);
#endif
            }
        }

        public override object GetModel()
        {
            return model;
        }

        public override void SetModel(object model)
        {
            this.model = (Model)model;
            ApplyCurrentModel();
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            ApplyCurrentModel();

            return null;
        }

        private void ApplyCurrentModel()
        {
            if (!string.IsNullOrEmpty(model.uuid))
            {
                Setup(scene, entity, model);
            }
        }
    }
}