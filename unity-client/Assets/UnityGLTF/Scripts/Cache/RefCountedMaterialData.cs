using UnityEngine;

namespace UnityGLTF.Cache
{
    public class RefCountedMaterialData : RefCountedBase
    {
        public Material material;
        public string crc;

        public RefCountedMaterialData(string crc, Material material)
        {
            this.material = material;
            this.crc = crc;
        }

        public override string ToString()
        {
            return "ref counted material. name = " + material.name + " ... crc = " + crc;
        }

        protected override void OnDestroyCachedData()
        {
            Debug.Log($"On Destroy -- {ToString()}");
            if (!string.IsNullOrEmpty(crc) && PersistentAssetCache.MaterialCacheByCRC.ContainsKey(crc))
                PersistentAssetCache.MaterialCacheByCRC.Remove(crc);

            UnityEngine.Object.Destroy(material);
        }
    }
}