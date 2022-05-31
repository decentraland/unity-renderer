using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Config3DDome : Config3DBase
    {
        public TextureLayer layers = new TextureLayer();
        public float domeRadius = SkyboxUtils.DOME_DEFAULT_SIZE;

        public Config3DDome(string name)
        {
            configType = Additional3DElements.Dome;
            layers = new TextureLayer(name);
        }

        public override bool IsConfigActive(float currentTime, float cycleTime = 24)
        {
            bool configActive = false;

            if (!enabled)
            {
                layers.renderType = LayerRenderType.NotRendering;
                return configActive;
            }


            float endTimeEdited = layers.timeSpan_End;
            float dayTimeEdited = currentTime;

            if (layers.timeSpan_End < layers.timeSpan_start)
            {
                endTimeEdited = cycleTime + layers.timeSpan_End;
                if (currentTime < layers.timeSpan_start)
                {
                    dayTimeEdited = cycleTime + currentTime;
                }
            }

            if (dayTimeEdited >= layers.timeSpan_start && dayTimeEdited <= endTimeEdited)
            {
                configActive = true;
                layers.renderType = LayerRenderType.Rendering;
            }
            else
            {
                layers.renderType = LayerRenderType.NotRendering;
            }

            return configActive;
        }

        public Config3DDome DeepCopy()
        {
            Config3DDome dome = (Config3DDome)this.MemberwiseClone();
            dome.layers = layers.DeepCopy();
            return dome;
        }
    }
}