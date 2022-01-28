﻿using System;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class ComponentTextureAttachment : ITextureAttachment
    {
        BaseDisposable component;

        private Action<ITextureAttachment> OnUpdateEvent;

        // As the SubscribeToEntityUpdates() method is not easy on the CPU, we only call it if the OnUpdateEvent
        // is actually used. this means that DCLVideoTexture will still work without incurring overhead to
        // DCLTexture components.
        public event Action<ITextureAttachment> OnUpdate
        {
            add
            {
                if ( OnUpdateEvent.GetInvocationList().Length == 0 )
                    SubscribeToEntityUpdates();

                OnUpdateEvent += value;
            }
            remove
            {
                OnUpdateEvent -= value;

                if ( OnUpdateEvent.GetInvocationList().Length == 0 )
                    UnsubscribeToEntityUpdates();
            }
        }

        private Action<ITextureAttachment> OnDetachEvent;

        public event Action<ITextureAttachment> OnDetach
        {
            add
            {
                if ( OnDetachEvent.GetInvocationList().Length == 0 )
                {
                    component.OnDetach -= OnComponentDetach;
                    component.OnDetach += OnComponentDetach;
                }

                OnDetachEvent += value;
            }
            remove
            {
                OnDetachEvent -= value;

                if ( OnDetachEvent.GetInvocationList().Length == 0 )
                {
                    component.OnDetach -= OnComponentDetach;
                }
            }
        }

        private Action<ITextureAttachment> OnAttachEvent;

        public event Action<ITextureAttachment> OnAttach
        {
            add
            {
                if ( OnAttachEvent.GetInvocationList().Length == 0 )
                {
                    component.OnAttach -= OnComponentAttach;
                    component.OnAttach += OnComponentAttach;
                }

                OnAttachEvent += value;
            }
            remove
            {
                OnAttachEvent -= value;

                if ( OnAttachEvent.GetInvocationList().Length == 0 )
                {
                    component.OnAttach -= OnComponentAttach;
                }
            }
        }

        public ComponentTextureAttachment(BaseDisposable component)
        {
            this.component = component;
        }

        public void Dispose()
        {
            component.OnAttach -= OnComponentAttach;
            component.OnDetach -= OnComponentDetach;
            UnsubscribeToEntityUpdates();
        }

        float ITextureAttachment.GetClosestDistanceSqr(Vector3 fromPosition)
        {
            float dist = int.MaxValue;
            if (component.attachedEntities.Count > 0)
            {
                using (var iterator = component.attachedEntities.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var entity = iterator.Current;
                        if (IsEntityVisible(entity))
                        {
                            var entityDist = (entity.meshRootGameObject.transform.position - fromPosition).sqrMagnitude;
                            if (entityDist < dist)
                                dist = entityDist;
                        }
                    }
                }
            }

            return dist;
        }

        bool ITextureAttachment.IsVisible()
        {
            if (component.attachedEntities.Count > 0)
            {
                using (var iterator = component.attachedEntities.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        if (IsEntityVisible(iterator.Current))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public string GetId()
        {
            return component.id;
        }

        bool IsEntityVisible(IDCLEntity entity)
        {
            if (entity.meshesInfo == null)
                return false;
            if (entity.meshesInfo.currentShape == null)
                return false;
            return entity.meshesInfo.currentShape.IsVisible();
        }

        private void OnComponentDetach(IDCLEntity obj)
        {
            OnDetachEvent?.Invoke(this);
        }

        private void OnComponentAttach(IDCLEntity obj)
        {
            OnAttachEvent?.Invoke(this);
        }

        private void OnEntityShapeUpdated(IDCLEntity entity)
        {
            OnUpdateEvent?.Invoke(this);
        }

        private void UnsubscribeToEntityUpdates()
        {
            if (component.attachedEntities.Count > 0)
            {
                using (var iterator = component.attachedEntities.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var entity = iterator.Current;
                        entity.OnShapeUpdated -= OnEntityShapeUpdated;
                    }
                }
            }
        }

        private void SubscribeToEntityUpdates()
        {
            if (component.attachedEntities.Count > 0)
            {
                using (var iterator = component.attachedEntities.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var entity = iterator.Current;
                        entity.OnShapeUpdated -= OnEntityShapeUpdated;
                        entity.OnShapeUpdated += OnEntityShapeUpdated;
                    }
                }
            }
        }
    }
}