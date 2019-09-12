using UnityEngine;
using UnityGLTF;

namespace DCL
{
    public class PoolInstantiator_GLTF : IPooledObjectInstantiator
    {
        public bool IsValid(GameObject original)
        {
            InstantiatedGLTFObject results = original.GetComponentInChildren<InstantiatedGLTFObject>(true);
            return results != null;
        }

        public GameObject Instantiate(GameObject original)
        {
            if (original == null)
            {
                Debug.LogError("PoolInstantiator_GLTF: Invalid original gameObject!");
                return new GameObject("_PoolInstantiator_Error");
            }

            InstantiatedGLTFObject gltfInstance = original.GetComponentInChildren<InstantiatedGLTFObject>(true);

            if (gltfInstance == null)
            {
                Debug.LogError("PoolInstantiator_GLTF: Invalid GLTF! Couldn't duplicate correctly.");
                GameObject result = Instantiate(original);
                result.name += "_Error";
                return result;
            }

            InstantiatedGLTFObject gltfDuplicate = null;

            gltfDuplicate = gltfInstance.Duplicate();

            return gltfDuplicate.gameObject;
        }
    }
}
