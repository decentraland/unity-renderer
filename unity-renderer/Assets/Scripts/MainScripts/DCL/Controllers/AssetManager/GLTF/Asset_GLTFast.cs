using System;
using GLTFast;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_GLTFast : Asset_WithPoolableContainer
    {
        private GltfImport gltfImport;
        public override GameObject container { get; set; }

        public Asset_GLTFast()
        {
            container = new GameObject("Asset_GLTFast Container");
        }

        public override object Clone()
        {
            Debug.Log("Cloned!");
            return base.Clone();
        }

        public void Setup(GltfImport gltfImport) { this.gltfImport = gltfImport; }
        
        public void Show(bool b, Action success)
        {
            container.SetActive(true);
            success?.Invoke();
        }
        public void Hide()
        {
            container.SetActive(false);
        }

        public override void Cleanup()
        {
            Object.Destroy(container);
        }

    }
}