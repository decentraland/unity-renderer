using UnityEngine;

namespace DCL
{
    public class Asset_Mesh : Asset
    {      
        public Mesh cubeMesh = null;
        
        public override void Cleanup()
        {
            GameObject.Destroy(cubeMesh);
        }
    }
}