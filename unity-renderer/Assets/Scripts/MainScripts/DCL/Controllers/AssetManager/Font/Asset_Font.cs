using DCL.Helpers;
using TMPro;
using UnityEngine;

namespace DCL
{
    public class Asset_Font : Asset
    {
        public TMP_FontAsset font;
        
        public override void Cleanup()
        {
            font = null;
        }
    }
}