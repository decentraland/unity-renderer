using Cysharp.Threading.Tasks;
using GLTFast;
using System;
using UnityEngine;

namespace DCL
{
    public class Asset_GLTFast_Loader : Asset
    {
        public GltfImport GltfImport { get; private set; }

        public void Setup(GltfImport importer)
        {
            GltfImport = importer;
        }

        public override void Cleanup()
        {
            GltfImport?.Dispose();
            GltfImport = null;
        }

        public async UniTask InstantiateAsync(Transform containerTransform)
        {
            if (GltfImport.SceneCount > 1)
                for (int i = 0; i < GltfImport.SceneCount; i++)
                {
                    var targetTransform = containerTransform;

                    if (i != 0)
                    {
                        var go = new GameObject($"{containerTransform.name}_{i}");
                        Transform goTransform = go.transform;
                        goTransform.SetParent(containerTransform, false);
                        targetTransform = goTransform;
                    }

                    await GltfImport.InstantiateSceneAsync(targetTransform, i);
                }
            else
                await GltfImport.InstantiateSceneAsync(containerTransform);
        }
    }
}
