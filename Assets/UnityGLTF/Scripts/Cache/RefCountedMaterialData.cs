using UnityEngine;

namespace UnityGLTF.Cache
{
    public class RefCountedMaterialData : RefCountedBase
    {
        public Material material;
        private string crc;

        public RefCountedMaterialData(string crc, Material material)
        {
            this.material = material;
            this.crc = crc;
        }

        protected override void OnDestroyCachedData()
        {
            if (!string.IsNullOrEmpty(crc) && PersistentAssetCache.MaterialCacheByCRC.ContainsKey(crc))
                PersistentAssetCache.MaterialCacheByCRC.Remove(crc);

            UnityEngine.Object.Destroy(material);
        }
    }
}
