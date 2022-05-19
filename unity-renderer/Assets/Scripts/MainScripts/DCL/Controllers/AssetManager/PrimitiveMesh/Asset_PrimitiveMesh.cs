using UnityEngine;

namespace DCL
{
    public class Asset_PrimitiveMesh : Asset
    {      
        public Mesh mesh = null;
        
        public override void Cleanup()
        {
            GameObject.Destroy(mesh);
        }
    }
}