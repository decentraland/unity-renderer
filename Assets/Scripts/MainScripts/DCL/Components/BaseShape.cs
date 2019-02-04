using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseShape : BaseDisposable
    {
        public override string componentName => "shape";

        public BaseShape(ParcelScene scene) : base(scene)
        {
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            if (attachedEntities.Contains(entity))
                return;

            if (entity.currentShape != null)
                entity.currentShape.DetachFrom(entity);

            entity.currentShape = this;

            base.AttachTo(entity);
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
            if (!attachedEntities.Contains(entity))
                return;

            base.DetachFrom(entity);
            // We do this instead of OnDetach += because it is required to run after every OnDetach listener
            entity.currentShape = null;
        }

        public static void ConfigureColliders(DecentralandEntity entity, bool hasCollision, bool filterByColliderName = false)
        {
            if (entity.meshGameObject == null) return;

            MeshCollider collider;
            MeshFilter[] meshFilters = entity.meshGameObject.GetComponentsInChildren<MeshFilter>();

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

