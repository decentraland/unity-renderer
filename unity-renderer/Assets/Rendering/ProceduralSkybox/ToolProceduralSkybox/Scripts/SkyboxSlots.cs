using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class SkyboxSlots
    {
        public bool disabled = false;
        public string slotName;
        public bool expandedInEditor;
        public float startTime = 0;
        public float endTime = 24;
        public List<TextureLayer> layers = new List<TextureLayer>();

        public SkyboxSlots(float startTime = 0, float endTime = 24)
        {
            disabled = false;
            this.startTime = startTime;
            this.endTime = endTime;
            layers = new List<TextureLayer>();
        }

        public TextureLayer GetActiveLayer(float currentTime)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (currentTime >= layers[i].timeSpan_start && currentTime <= layers[i].timeSpan_End)
                {
                    return layers[i];
                }
            }
            return null;
        }
    }
}