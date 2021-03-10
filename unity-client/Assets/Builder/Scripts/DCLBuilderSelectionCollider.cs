using UnityEngine;
using DCL.Helpers;

namespace Builder
{
    public class DCLBuilderSelectionCollider : MonoBehaviour
    {
        public const string LAYER_BUILDER_POINTER_CLICK = "OnBuilderPointerClick";

        public DCLBuilderEntity ownerEntity { get; private set; }

        private Mesh meshColliderForSkinnedMesh = null;

        public void Initialize(DCLBuilderEntity builderEntity, Renderer renderer)
        {
            ownerEntity = builderEntity;

            gameObject.layer = LayerMask.NameToLayer(LAYER_BUILDER_POINTER_CLICK);

            Transform t = gameObject.transform;
            t.SetParent(renderer.transform);
            t.ResetLocalTRS();

            var meshCollider = gameObject.AddComponent<MeshCollider>();

            if (renderer is SkinnedMeshRenderer)
            {
                if (meshColliderForSkinnedMesh)
                {
                    Object.Destroy(meshColliderForSkinnedMesh);
                }
                meshColliderForSkinnedMesh = new Mesh();
                (renderer as SkinnedMeshRenderer).BakeMesh(meshColliderForSkinnedMesh);
                meshCollider.sharedMesh = meshColliderForSkinnedMesh;
                t.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
            }
            else
            {
                meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            }
            meshCollider.enabled = renderer.enabled;
        }

        private void OnDestroy()
        {
            if (meshColliderForSkinnedMesh)
            {
                Object.Destroy(meshColliderForSkinnedMesh);
            }
        }
    }
}