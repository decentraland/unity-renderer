using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class CopyFunctionality
    {
        private TextureLayer copiedLayer;

        public bool IsTextureLayerAvailable() { return copiedLayer != null; }

        public bool IsDomeAvailable() { return copiedLayer != null; }

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