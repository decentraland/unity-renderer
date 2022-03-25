using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class CopyFunctionality
    {
        public TextureLayer copiedLayer;

        public bool IsTextureLayerAvailable()
        {
            if (copiedLayer == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetTextureLayer(TextureLayer layer) { copiedLayer = layer; }

        public TextureLayer GetCopiedTextureLayer() { return copiedLayer; }
    }
}