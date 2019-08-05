using UnityEngine;
using UnityGLTF;

namespace DCL
{
    public class PoolInstantiator_GLTF : IPooledObjectInstantiator
    {
        public GameObject Instantiate(GameObject original)
        {
            InstantiatedGLTFObject gltfInstance = original.GetComponentInChildren<InstantiatedGLTFObject>(true);
            InstantiatedGLTFObject gltfDuplicate = null;

            gltfDuplicate = gltfInstance.Duplicate();

            return gltfDuplicate.gameObject;
        }
    }
}
