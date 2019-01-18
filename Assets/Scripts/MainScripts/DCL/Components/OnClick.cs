using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;
using DCL.Helpers;
using DCL.Models;
using DCL.Controllers;

namespace DCL.Components
{

    public abstract class UUIDComponent : MonoBehaviour
    {
        public string type;
        public string uuid;
        public abstract void Setup(ParcelScene scene, DecentralandEntity entity, string uuid, string type);

        public static void SetForEntity(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
            switch (type)
            {
                case "onClick":
                    SetUpComponent<OnClickComponent>(scene, entity, uuid, type);
                    return;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            throw new UnityException($"Cannot create UUIDComponent of type '{type}'.");
#endif

        }

        public static void RemoveFromEntity(DecentralandEntity entity, string type)
        {
            switch (type)
            {
                case "onClick":
                    RemoveComponent<OnClickComponent>(entity);
                    break;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            throw new UnityException($"Cannot remove UUIDComponent of type '{type}'.");
#endif
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
            var currentComponent = DCL.Helpers.Utils.GetOrCreateComponent<T>(entity.gameObject);

            currentComponent.Setup(scene, entity, uuid, type);
        }
    }

    public class OnClickComponent : UUIDComponent
    {

        Rigidbody rigidBody;
        GameObject[] OnClickColliderGameObjects;
        DecentralandEntity entity;
        ParcelScene scene;

        public override void Setup(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
            this.entity = entity;
            this.scene = scene;
            this.uuid = uuid;
            this.type = type;

            if (entity.meshGameObject && entity.meshGameObject.GetComponentInChildren<MeshFilter>() != null)
            {
                Initialize();
            }

            entity.OnShapeUpdated -= OnComponentUpdated;
            entity.OnShapeUpdated += OnComponentUpdated;
        }

        public void Initialize()
        {
            if (!entity.meshGameObject || entity.meshGameObject.GetComponentInChildren<MeshFilter>() == null)
            {
                return;
            }

            // we add a rigidbody to be able to detect the children colliders for the OnClick functionality
            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                rigidBody = gameObject.AddComponent<Rigidbody>();
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }

            // Create OnClickCollider child
            var meshFilters = GetComponentsInChildren<MeshFilter>();
            var onClickColliderObjectName = "OnClickCollider";
            var onClickColliderObjectLayer = LayerMask.NameToLayer("OnClick");

            DestroyOnClickColliders();

            OnClickColliderGameObjects = new GameObject[meshFilters.Length];
            for (int i = 0; i < OnClickColliderGameObjects.Length; i++)
            {
                OnClickColliderGameObjects[i] = new GameObject();
                OnClickColliderGameObjects[i].name = onClickColliderObjectName;
                OnClickColliderGameObjects[i].layer = onClickColliderObjectLayer; // to avoid movement collisions with its collider

                var meshCollider = OnClickColliderGameObjects[i].AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilters[i].sharedMesh;

                // Reset objects position, rotation and scale once it's been parented
                OnClickColliderGameObjects[i].transform.SetParent(meshFilters[i].transform);
                OnClickColliderGameObjects[i].transform.localScale = Vector3.one;
                OnClickColliderGameObjects[i].transform.localRotation = Quaternion.identity;
                OnClickColliderGameObjects[i].transform.localPosition = Vector3.zero;
            }
        }

        void OnComponentUpdated()
        {
            Initialize();
        }

        // Unity hook
        void OnMouseDown()
        {
            if (!enabled) return;

            int mouseButtonPressed = 0;
            if (Input.GetMouseButton(1))
            {
                mouseButtonPressed = 1;
            }

            DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, uuid, mouseButtonPressed);
        }

        void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;

            Destroy(rigidBody);

            DestroyOnClickColliders();
        }

        void DestroyOnClickColliders()
        {
            if (OnClickColliderGameObjects == null) return;

            for (int i = 0; i < OnClickColliderGameObjects.Length; i++)
            {
                Destroy(OnClickColliderGameObjects[i]);
            }
        }
    }
}
