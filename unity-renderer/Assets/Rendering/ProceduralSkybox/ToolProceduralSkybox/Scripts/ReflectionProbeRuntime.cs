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
        [SerializeField] bool fixedSkybox = false;
        [SerializeField] bool fixedSkyboxBaked = false;
        [SerializeField] float fixedSkyboxUpdateTime = 3;
        [SerializeField] float fixedSkyboxTimer = 0;

        private void Start()
        {
            timer = 0;
            realtimeProbe.resolution = resolution;
            BakeNewReflection();
        }

        private void Update()
        {
            if (fixedSkybox)
            {
                UpdateFixedSkybox();
            }
            else
            {
                UpdateDynamicSkybox();
            }


        }

        void UpdateFixedSkybox()
        {
            if (fixedSkyboxBaked)
            {
                return;
            }

            fixedSkyboxTimer += Time.deltaTime;

            if (fixedSkyboxTimer >= fixedSkyboxUpdateTime)
            {
                BakeNewReflection();
                fixedSkyboxBaked = true;
            }
        }

        void UpdateDynamicSkybox()
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

            if (fixedSkybox)
            {
                FixedSkyboxTimeChanged();
            }
            else
            {
                timer = 0;
                BakeNewReflection();
            }
        }

        public void SkyboxModeChanged(bool dynamicSkybox)
        {
            fixedSkybox = !dynamicSkybox;

            if (fixedSkybox)
            {
                fixedSkyboxTimer = 0;
                fixedSkyboxBaked = false;
            }
            else
            {
                timer = 0;
                BakeNewReflection();
            }
        }

        public void FixedSkyboxTimeChanged()
        {
            fixedSkyboxTimer = 0;
            fixedSkyboxBaked = false;
        }
    }
}