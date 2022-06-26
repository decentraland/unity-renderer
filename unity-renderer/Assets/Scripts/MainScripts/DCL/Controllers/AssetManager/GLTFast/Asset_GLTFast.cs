using GLTFast;
using UnityEngine;

namespace DCL
{
    public class Asset_GLTFast : Asset
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
            gltfImport.InstantiateMainScene(containerTransform);
        }
    }
}