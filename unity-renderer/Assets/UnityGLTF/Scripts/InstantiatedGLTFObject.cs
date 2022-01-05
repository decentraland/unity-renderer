using System.Collections.Generic;
using GPUSkinning;
using UnityEngine;
using UnityGLTF.Cache;

namespace UnityGLTF
{
    /// <summary>
    /// Instantiated GLTF Object component that gets added to the root of every GLTF game object created by a scene importer.
    /// </summary>
    public class InstantiatedGLTFObject : MonoBehaviour
    {
        /// <summary>
        /// Ref-counted cache data for this object.
        /// The same instance of this cached data will be used for all copies of this GLTF object,
        /// and the data gets cleaned up when the ref counts goes to 0.
        /// </summary>
        private RefCountedCacheData cachedData;
        
        public List<SimpleGPUSkinning> gpuSkinnings;

        public RefCountedCacheData CachedData
        {
            get { return cachedData; }

            set
            {
                if (cachedData != value)
                {
                    if (cachedData != null)
                    {
                        cachedData.DecreaseRefCount();
                    }

                    cachedData = value;

                    if (cachedData != null)
                    {
                        cachedData.IncreaseRefCount();
                    }
                }
            }
        }

        /// <summary>
        /// Duplicates the instantiated GLTF object.
        /// Note that this should always be called if you intend to create a new instance of a GLTF object, 
        /// in order to properly preserve the ref count of the dynamically loaded mesh data, otherwise
        /// you will run into a memory leak due to non-destroyed meshes, textures and materials.
        /// </summary>
        /// <returns></returns>
        public InstantiatedGLTFObject Duplicate()
        {
            GameObject duplicatedObject = Instantiate(gameObject);

            InstantiatedGLTFObject newGltfObjectComponent = duplicatedObject.GetComponent<InstantiatedGLTFObject>();
            newGltfObjectComponent.CachedData = CachedData;
            
            // newGltfObjectComponent.SetupGPUSkinning();

            return newGltfObjectComponent;
        }
        
        /*public void SetupGPUSkinning()
        {
            gpuSkinnings.Clear();
            
            SkinnedMeshRenderer[] renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                gpuSkinnings.Add(new SimpleGPUSkinning(renderers[i] as SkinnedMeshRenderer, true, 1, 3));
            }
        }*/

        private void LateUpdate()
        {
            if (gpuSkinnings == null) return;
            
            int count = gpuSkinnings.Count;
            for (var i = 0; i < count; i++)
            {
                gpuSkinnings[i].Update();
            }
        }

        private void OnDestroy()
        {
            CachedData = null;
        }
    }
}