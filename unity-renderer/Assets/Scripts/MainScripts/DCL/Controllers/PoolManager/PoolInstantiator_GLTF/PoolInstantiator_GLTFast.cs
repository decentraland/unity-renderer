using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class PoolInstantiator_GLTFast : IPooledObjectInstantiator
    {
        public bool IsValid(GameObject original)
        {
            return true;
        }

        public GameObject Instantiate(GameObject original)
        {
            Debug.Log($"Instantiating a new one");

            if (original == null)
            {
                Debug.LogError("PoolInstantiator_GLTFast: Invalid original gameObject!");
                return new GameObject("_PoolInstantiator_Error");
            }

            return Object.Instantiate(original);
        }
    }
}