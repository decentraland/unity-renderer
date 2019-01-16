using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseShape : BaseComponent
    {
        public override string componentName => "shape";

        [HideInInspector] public GameObject meshGameObject;

        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        protected virtual void Awake()
        {
            // clear out previous shape if it exists
            // it would be better to check for a pre-existent shape component with GetComponent<>() but we can't while using the "Template" pattern on these classes
            var updateableComponents = GetComponents<UpdateableComponent>();
            for (int i = 0; i < updateableComponents.Length; i++)
            {
                if (updateableComponents[i] == this) continue;

                if (Utils.IsShapeComponent(updateableComponents[i]))
                {
                    meshGameObject = transform.Find(MESH_GAMEOBJECT_NAME).gameObject; // it would be better to get the meshGameObject reference from the previous shape, but we can't while using the "Template" pattern on these classes

                    Destroy(updateableComponents[i]);

                    break; // There won't be more than 1 shape component at a time so we escape the loop after destroying it.
                }
            }

            if (meshGameObject == null)
            {
                meshGameObject = new GameObject();
                meshGameObject.name = MESH_GAMEOBJECT_NAME;
                meshGameObject.transform.SetParent(transform);
                meshGameObject.transform.localPosition = Vector3.zero;
            }

            meshFilter = meshGameObject.GetComponent<MeshFilter>();
            meshRenderer = meshGameObject.GetComponent<MeshRenderer>();
        }

        protected virtual void OnDestroy()
        {
            /*
             * Destruction is not immediate, so we check if a NEW non-gltf-non-obj-shape has already been added to the gameobject,
             * in that case we avoid destroying the meshGameObject (gltf/obj don't use the meshGameObject), as it will be reused.
             */
            var updateableComponents = GetComponents<UpdateableComponent>();
            for (int i = 0; i < updateableComponents.Length; i++)
            {
                if (updateableComponents[i] == this) continue;

                if (Utils.IsShapeComponent(updateableComponents[i])) return;
            }

            if (meshGameObject != null)
            {
                Destroy(meshGameObject);
            }
        }

        public GameObject AttachPlaceholderRendererGameObject(UnityEngine.Transform targetTransform)
        {
            var placeholderRenderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();

            placeholderRenderer.material = Resources.Load<Material>("Materials/AssetLoading");
            placeholderRenderer.transform.SetParent(targetTransform);
            placeholderRenderer.transform.localPosition = Vector3.zero;
            placeholderRenderer.name = "PlaceholderRenderer";

            return placeholderRenderer.gameObject;
        }

        protected void ConfigureCollision(bool hasCollision, bool filterByColliderName = false)
        {
            if (meshGameObject == null) return;

            MeshCollider collider;
            MeshFilter[] meshFilters = meshGameObject.GetComponentsInChildren<MeshFilter>();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (filterByColliderName)
                {
                    if (!meshFilters[i].transform.parent.name.EndsWith("_collider")) continue;

                    meshFilters[i].GetComponent<MeshRenderer>().enabled = false;
                    meshFilters[i].gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilters[i].sharedMesh;
                }
                else
                {
                    collider = meshFilters[i].GetComponent<MeshCollider>();

                    if (hasCollision)
                    {
                        if (collider == null)
                        {
                            collider = meshFilters[i].gameObject.AddComponent<MeshCollider>();
                        }
                        else if (!collider.enabled)
                        {
                            collider.enabled = true;
                        }

                        collider.sharedMesh = meshFilters[i].sharedMesh;
                    }
                    else if (collider != null && collider.enabled)
                    {
                        collider.enabled = false;
                    }
                }
            }
        }
    }
}
