using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class UUIDComponent : BaseComponent
    {
        public override string componentName => "UUIDComponent";

        public class Model
        {
            public string type;
            public string uuid;
        }

        public Model model;

        public virtual void Setup(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
        }

        public static void SetForEntity(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
            switch (type)
            {
                case "onClick":
                    SetUpComponent<OnClickComponent>(scene, entity, uuid, type);
                    return;
            }

            Debug.LogWarning($"Cannot create UUIDComponent of type '{type}'.");
        }

        public static void RemoveFromEntity(DecentralandEntity entity, string type)
        {
            switch (type)
            {
                case "onClick":
                    RemoveComponent<OnClickComponent>(entity);
                    break;
            }

            Debug.LogWarning($"Cannot remove UUIDComponent of type '{type}'.");
        }

        private static void RemoveComponent<T>(DecentralandEntity entity) where T : UUIDComponent
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

        private static void SetUpComponent<T>(ParcelScene scene, DecentralandEntity entity, string uuid, string type) where T : UUIDComponent
        {
            var currentComponent = entity.gameObject.GetOrCreateComponent<T>();

            currentComponent.Setup(scene, entity, uuid, type);
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            if (!string.IsNullOrEmpty(model.uuid))
                SetForEntity(scene, entity, model.uuid, model.type);

            yield return null;
        }
    }
}
