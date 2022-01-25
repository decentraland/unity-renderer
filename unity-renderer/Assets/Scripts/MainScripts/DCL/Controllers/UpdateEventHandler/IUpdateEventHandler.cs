﻿using System;

namespace DCL
{
    public interface IUpdateEventHandler : IService
    {
        public enum EventType
        {
            Update,
            LateUpdate,
            FixedUpdate,
            OnGui
        }

        void AddListener( EventType eventType, Action action );
        void RemoveListener( EventType eventType, Action action );
    }
}