using UnityEngine;

namespace DCL
{
    public class Asset_BoxShape : Asset
    {      
        public Mesh cubeMesh = null;
        
        public override void Cleanup()
        {
            GameObject.Destroy(cubeMesh);
        }
    }
}