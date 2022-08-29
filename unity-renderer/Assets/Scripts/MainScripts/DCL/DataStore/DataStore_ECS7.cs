using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.UI;
using DCL.Helpers;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace DCL
{
    public class DataStore_ECS7
    {
        public readonly struct PointerEvent
        {
            public readonly int buttonId;
            public readonly RaycastResultInfo rayResult;
            public readonly bool isButtonDown;

            public PointerEvent(int buttonId, bool isButtonDown, RaycastResultInfo rayResult)
            {
                this.buttonId = buttonId;
                this.isButtonDown = isButtonDown;
                this.rayResult = rayResult;
            }
        }
        
        public readonly BaseList<IParcelScene> scenes = new BaseList<IParcelScene>();
        public readonly BaseDictionary<string, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<string, BaseRefCountedCollection<object>>();
        public readonly BaseDictionary<long, List<IPointerInputEvent>> entityEvents = new BaseDictionary<long, List<IPointerInputEvent>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
        public IUIDataContainer uiDataContainer = new UIDataContainer();
        public bool isEcs7Enable = false;
        public PointerEvent? lastPointerInputEvent = null;
    }
}