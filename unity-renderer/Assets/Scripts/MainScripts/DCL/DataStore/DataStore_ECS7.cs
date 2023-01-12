using DCL.Controllers;
using DCL.Interface;
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
        }

        public readonly BaseList<IParcelScene> scenes = new BaseList<IParcelScene>();
        public readonly BaseDictionary<int, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<int, BaseRefCountedCollection<object>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
        public bool isEcs7Enabled = false;
        public RaycastEvent lastPointerRayHit = new RaycastEvent();

        // Should the max buttonId be defined by WebInterface?
        private const int MAX_BUTTON = (int)WebInterface.ACTION_BUTTON.ACTION_6;
        public bool[] buttonState = new bool[MAX_BUTTON];
        public bool[] lastButtonState = new bool[MAX_BUTTON];
    }
}
