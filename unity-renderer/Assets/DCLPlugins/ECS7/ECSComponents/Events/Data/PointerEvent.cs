﻿using DCL.ECSComponents;

namespace DCLPlugins.ECSComponents.Events
{
    public struct PointerEvent
    {
        public string sceneId;
        public ActionButton button;
        public RaycastHit hit;
        public StateEnum state;
        public int timestamp;
        public float analog;
    }
}