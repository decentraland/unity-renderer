using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class ReflectionProbeRuntime : MonoBehaviour
    {
        public ReflectionProbe realtimeProbe;
        public ReflectionProbe customProbe;
        public float updateAfter = 60;
        float timer;
        RenderTexture renderTex;
        public Transform followTransform;

        private void Start()
        {
            renderTex = new RenderTexture(realtimeProbe.resolution, realtimeProbe.resolution, 0);
            renderTex.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            timer = 0;
            BakeNewReflection();
        }

        private void Update()
        {
            timer += Time.deltaTime / updateAfter;

            if (timer >= 1)
            {
                timer = 0;
                BakeNewReflection();
            }

        }

        private void LateUpdate()
        {
            if (followTransform != null)
            {
                transform.position = followTransform.position;
            }
        }

        void BakeNewReflection()
        {
            realtimeProbe.RenderProbe();
            //customProbe.customBakedTexture = renderTex;
            Debug.Log("Procedural Skybox :: Render Probe");
        }
    }
}