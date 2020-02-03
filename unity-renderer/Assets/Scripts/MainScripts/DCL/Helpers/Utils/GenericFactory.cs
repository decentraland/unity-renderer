using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers
{
    [CreateAssetMenu(fileName = "GenericFactory", menuName = "GenericFactory")]
    public class GenericFactory : ScriptableObject
    {
        [System.Serializable]
        public class Map
        {
            public string key;
            public GameObject prefab;
        }

        public GameObject defaultPrefab;
        public Map[] items;

        private Dictionary<string, GameObject> cachedItems;

        private void PrepareCache()
        {
            if (cachedItems != null) return;

            cachedItems = new Dictionary<string, GameObject>();
            for (int i = 0; i < items.Length; i++)
            {
                if (!cachedItems.ContainsKey(items[i].key))
                {
                    cachedItems.Add(items[i].key, items[i].prefab);
                }
            }
        }

        public GameObject Instantiate(string key, Transform parent = null)
        {
            PrepareCache();

            GameObject prefab = cachedItems.ContainsKey(key) ? cachedItems[key] : defaultPrefab;
            if (prefab == null)
            {
                Debug.LogError($"Prefab for Key: {key} is null!");
                return default;
            }

            return parent == null ? Instantiate(prefab) : Instantiate(prefab, parent);
        }

        public T Instantiate<T>(string key, Transform parent = null) where T : Component
        {
            var prefab = Instantiate(key, parent);
            return prefab == null ? null : prefab.GetComponent<T>();
        }
    }
}