using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class CopyFunctionality
    {
        private TextureLayer copiedLayer;

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

        public bool IsDomeAvailable()
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

        public void SetDome(Config3DDome dome) { copiedLayer = dome.layers; }

        public Config3DDome GetCopiedDome()
        {
            Config3DDome dome = new Config3DDome(copiedLayer.nameInEditor);
            dome.layers = copiedLayer;
            return dome;
        }
    }
}