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

        private void Start()
        {
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

        void BakeNewReflection() { realtimeProbe.RenderProbe(); }
    }
}