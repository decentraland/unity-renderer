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
    public class OnClickComponent : UUIDComponent
    {
        Rigidbody rigidBody;
        GameObject[] OnClickColliderGameObjects;

        public override void Setup(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
            this.entity = entity;
            this.scene = scene;
            this.model.uuid = uuid;
            this.model.type = type;

            Initialize();

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

        void OnComponentUpdated(DecentralandEntity e)
        {
            Initialize();
        }

        // Unity hook
        public void OnMouseDown()
        {
            if (!enabled) return;

            DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
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
