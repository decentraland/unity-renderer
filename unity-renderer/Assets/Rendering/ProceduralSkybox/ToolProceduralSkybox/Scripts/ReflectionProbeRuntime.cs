using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public class ReflectionProbeRuntime : MonoBehaviour
    {
        public ReflectionProbe realtimeProbe;
        public float updateAfter = 60;
        float timer;
        public Transform followTransform;
        public int resolution = 256;

        private void Start()
        {
            timer = 0;
            realtimeProbe.resolution = resolution;
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
            realtimeProbe.resolution = resolution;
            realtimeProbe.RenderProbe();
        }

        public void UpdateResolution(int resolution)
        {
            this.resolution = resolution;
            realtimeProbe.resolution = resolution;
        }
    }
}