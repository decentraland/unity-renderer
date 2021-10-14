using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class SkyboxSlots
    {
        public bool enabled = true;
        public int slotID = -1;
        public string slotName;
        public bool expandedInEditor;
        public float startTime = 0;
        public float endTime = 24;
        public List<TextureLayer> layers = new List<TextureLayer>();

        public SkyboxSlots(int slotID, float startTime = 0, float endTime = 24)
        {
            this.slotID = slotID;
            enabled = true;
            this.startTime = startTime;
            this.endTime = endTime;
            layers = new List<TextureLayer>();
        }

        public void UpdateSlotsID(int slotID) { this.slotID = slotID; }

        public void AddNewLayer(TextureLayer layer, bool addInFront = false)
        {
            if (layer == null)
            {
                return;
            }

            if (addInFront)
            {
                this.layers.Insert(0, layer);
            }
            else
            {
                this.layers.Add(layer);
            }
        }

        public TextureLayer GetActiveLayer(float currentTime)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].timeSpan_End < layers[i].timeSpan_start)
                {
                    if (currentTime >= layers[i].timeSpan_start || currentTime <= layers[i].timeSpan_End)
                    {
                        if (layers[i].enabled)
                        {
                            return layers[i];
                        }
                    }
                }
                else
                {
                    if (currentTime >= layers[i].timeSpan_start && currentTime <= layers[i].timeSpan_End)
                    {
                        if (layers[i].enabled)
                        {
                            return layers[i];
                        }
                    }
                }
            }
            return null;
        }
    }
}