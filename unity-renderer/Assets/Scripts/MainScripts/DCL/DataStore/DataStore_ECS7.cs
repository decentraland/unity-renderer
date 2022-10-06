using DCL.Controllers;
using DCL.ECS7.UI;
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
        public readonly BaseDictionary<string, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<string, BaseRefCountedCollection<object>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
        public IUIDataContainer uiDataContainer = new UIDataContainer();
        public bool isEcs7Enabled = false;
        public PointerEvent lastPointerInputEvent = new PointerEvent();
        public RaycastEvent lastPointerRayHit = new RaycastEvent();
    }
}