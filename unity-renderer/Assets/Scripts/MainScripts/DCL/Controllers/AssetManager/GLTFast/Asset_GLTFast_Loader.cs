using GLTFast;
using UnityEngine;

namespace DCL
{
    public class Asset_GLTFast_Loader : Asset
    {
        public GltfImport gltfImport;
        public void Setup(GltfImport importer)
        {
            gltfImport = importer;
        }
        
        public override void Cleanup()
        {
            gltfImport?.Dispose();
            gltfImport = null;
        }

        public void Instantiate(Transform containerTransform)
        {
            if (gltfImport.sceneCount > 1)
            {
                for (int i = 0; i < gltfImport.sceneCount; i++)
                {
                    var targetTransform = containerTransform;
                    if (i != 0)
                    {
                        var go = new GameObject($"{containerTransform.name}_{i}");
                        Transform goTransform = go.transform;
                        goTransform.SetParent(containerTransform.parent, false);
                        targetTransform = goTransform;
                    }
                    
                    gltfImport.InstantiateScene(targetTransform, i);
                }
            }
            else
            {
                gltfImport.InstantiateScene(containerTransform);
            }
        }
    }
}