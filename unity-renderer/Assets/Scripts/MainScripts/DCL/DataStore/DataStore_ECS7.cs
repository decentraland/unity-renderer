using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DataStore_ECS7
    {
        public class PointerEvent
        {
            public int buttonId = 0;
            public bool isButtonDown = false;
            public bool hasValue = false;
        }

        public class RaycastEvent
        {
            public class Hit
            {
                public float distance;
                public Vector3 normal;
                public Vector3 point;
                public Collider collider;
            }

            public Hit hit = new Hit();
            public Ray ray;
            public bool didHit;
            public bool hasValue = false;

            public void UpdateByHitInfo(RaycastHit hitInfo, bool didHit, Ray ray)
            {
                hit.collider = hitInfo.collider;
                hit.point = hitInfo.point;
                hit.normal = hitInfo.normal;
                hit.distance = hitInfo.distance;

                this.didHit = didHit;
                this.ray = ray;

                hasValue = true;
            }
        }

        public readonly BaseList<IParcelScene> scenes = new BaseList<IParcelScene>();
        public readonly BaseDictionary<int, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<int, BaseRefCountedCollection<object>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
        public bool isEcs7Enabled = false;
        public RaycastEvent lastPointerRayHit = new RaycastEvent();

        // Input action state: true = pressing - false = released
        public readonly bool[] inputActionState = new bool[Enum.GetValues(typeof(WebInterface.ACTION_BUTTON)).Length];
    }
}
