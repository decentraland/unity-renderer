using UnityEngine;
using System.Collections.Generic;
using DCL.Models;
using DCL.Helpers;

namespace DCL.Controllers
{
    public class SceneBoundariesDebugModeChecker : SceneBoundariesChecker
    {
        class InvalidMeshInfo
        {
            public Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
            public GameObject wireframeObject;
        }

        static string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";

        Material invalidMeshMaterial;
        Dictionary<GameObject, InvalidMeshInfo> invalidMeshesOriginalMaterial;

        public SceneBoundariesDebugModeChecker(ParcelScene ownerScene) : base(ownerScene)
        {
            invalidMeshesOriginalMaterial = new Dictionary<GameObject, InvalidMeshInfo>();
            invalidMeshMaterial = Resources.Load("Materials/InvalidMesh") as Material;
        }

        protected override void UpdateEntityMeshesValidState(DecentralandEntity entity, bool isInsideBoundaries, Bounds meshBounds)
        {
            bool wasInvalid = invalidMeshesOriginalMaterial.ContainsKey(entity.gameObject);

            if(isInsideBoundaries)
            {
                if(wasInvalid)
                {
                    // Reset object materials
                    for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
                    {
                        entity.meshesInfo.renderers[i].sharedMaterial = invalidMeshesOriginalMaterial[entity.gameObject].originalMaterials[entity.meshesInfo.renderers[i]];
                    }

                    Utils.SafeDestroy(invalidMeshesOriginalMaterial[entity.gameObject].wireframeObject);

                    invalidMeshesOriginalMaterial.Remove(entity.gameObject);
                }
            }
            else if(!wasInvalid)
            {
                InvalidMeshInfo meshInfo = new InvalidMeshInfo();

                // Apply invalid material
                for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
                {
                    // Save original materials
                    meshInfo.originalMaterials.Add(entity.meshesInfo.renderers[i], entity.meshesInfo.renderers[i].sharedMaterial);

                    entity.meshesInfo.renderers[i].sharedMaterial = invalidMeshMaterial;
                }

                // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent) 
                // overload because we need to set the position and scale before parenting, to deal with scaled objects)
                meshInfo.wireframeObject = GameObject.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
                meshInfo.wireframeObject.transform.position = meshBounds.center;
                meshInfo.wireframeObject.transform.localScale = meshBounds.size;
                meshInfo.wireframeObject.transform.SetParent(entity.gameObject.transform);

                invalidMeshesOriginalMaterial.Add(entity.gameObject, meshInfo);
            }
        }
    }
}