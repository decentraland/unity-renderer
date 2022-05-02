using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class Asset_Material : Asset
    {
        public Material material;
        public override void Cleanup()
        {
            if (material != null)
            {
                Utils.SafeDestroy(material);
            }
        }
    }
}