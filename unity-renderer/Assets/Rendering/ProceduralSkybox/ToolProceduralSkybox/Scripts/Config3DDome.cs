using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Config3DDome : Config3DBase
    {
        public BackgroundLayer backgroundLayer = new BackgroundLayer();
        public List<TextureLayer> layers = new List<TextureLayer>();

        public Config3DDome(string name)
        {
            configType = Additional3DElements.Dome;
            nameInEditor = name;
        }
    }

    [System.Serializable]
    public class BackgroundLayer
    {
        // Background Color
        public Gradient skyColor = new Gradient();
        public Gradient horizonColor = new Gradient();
        public Gradient groundColor = new Gradient();

        // Horizon Layer
        public List<TransitioningFloat> horizonWidth = new List<TransitioningFloat>();
        public List<TransitioningFloat> horizonHeight = new List<TransitioningFloat>();
        public Texture2D horizonMask;
        public Vector3 horizonMaskValues = new Vector3(0, 0, 0);
    }
}