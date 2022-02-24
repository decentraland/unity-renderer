using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Config3DDome : Config3DBase
    {
        public List<TextureLayer> layers = new List<TextureLayer>();

        public Config3DDome(string name)
        {
            configType = Additional3DElements.Dome;
            nameInEditor = name;
        }
    }
}