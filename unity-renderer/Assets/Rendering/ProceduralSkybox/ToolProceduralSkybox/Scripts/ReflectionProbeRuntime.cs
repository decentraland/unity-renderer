using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class ReflectionProbeRuntime : MonoBehaviour
    {
        public ReflectionProbe environmentProbe;
        public float updateAfter = 60;
        float timer;
        RenderTexture renderTex;

        private void Update()
        {
            timer += Time.deltaTime / updateAfter;

            if (timer >= 1)
            {
                timer = 0;
                environmentProbe.RenderProbe();
                Debug.Log("Procedural Skybox :: Render Probe");
            }
        }
    }
}