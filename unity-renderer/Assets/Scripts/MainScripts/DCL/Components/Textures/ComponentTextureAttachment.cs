using System;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class ComponentTextureAttachment : ITextureAttachment
    {
        public ISharedComponent component => disposableComponent;

        private BaseDisposable disposableComponent;

        private Action<ITextureAttachment> OnUpdateEvent;

        // As the SubscribeToEntityUpdates() method is not easy on the CPU, we only call it if the OnUpdateEvent
        // is actually used. this means that DCLVideoTexture will still work without incurring overhead to
        // DCLTexture components.
        public event Action<ITextureAttachment> OnUpdate
        {
            add
            {
                if ( OnUpdateEvent == null || OnUpdateEvent.GetInvocationList().Length == 0 )
                    SubscribeToEntityUpdates();

                OnUpdateEvent += value;
            }
            remove
            {
                OnUpdateEvent -= value;

                if ( OnUpdateEvent != null && OnUpdateEvent.GetInvocationList().Length == 0 )
                    UnsubscribeToEntityUpdates();
            }
        }

        private Action<ITextureAttachment> OnDetachEvent;

        public event Action<ITextureAttachment> OnDetach
        {
            add
            {
                if ( OnDetachEvent == null || OnDetachEvent.GetInvocationList().Length == 0 )
                {
                    disposableComponent.OnDetach -= OnComponentDetach;
                    disposableComponent.OnDetach += OnComponentDetach;
                }

                OnDetachEvent += value;
            }
            remove
            {
                OnDetachEvent -= value;

                if ( OnDetachEvent != null && OnDetachEvent.GetInvocationList().Length == 0 )
                {
                    disposableComponent.OnDetach -= OnComponentDetach;
                }
            }
        }

        private Action<ITextureAttachment> OnAttachEvent;

        public event Action<ITextureAttachment> OnAttach
        {
            add
            {
                if ( OnAttachEvent == null || OnAttachEvent.GetInvocationList().Length == 0 )
                {
                    disposableComponent.OnAttach -= OnComponentAttach;
                    disposableComponent.OnAttach += OnComponentAttach;
                }

                OnAttachEvent += value;
            }
            remove
            {
                OnAttachEvent -= value;

                if ( OnAttachEvent != null && OnAttachEvent.GetInvocationList().Length == 0 )
                {
                    disposableComponent.OnAttach -= OnComponentAttach;
                }
            }
        }

        public ComponentTextureAttachment(BaseDisposable component)
        {
            this.disposableComponent = component;
        }

        public void Dispose()
        {
            disposableComponent.OnAttach -= OnComponentAttach;
            disposableComponent.OnDetach -= OnComponentDetach;
            UnsubscribeToEntityUpdates();
        }

        float ITextureAttachment.GetClosestDistanceSqr(Vector3 fromPosition)
        {
            float dist = int.MaxValue;

            if (disposableComponent.attachedEntities.Count > 0)
            {
                using (var iterator = disposableComponent.attachedEntities.GetEnumerator())
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
            if (disposableComponent.attachedEntities.Count > 0)
            {
                using (var iterator = disposableComponent.attachedEntities.GetEnumerator())
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
            return disposableComponent.id;
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
            if (disposableComponent.attachedEntities.Count > 0)
            {
                using (var iterator = disposableComponent.attachedEntities.GetEnumerator())
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
            if (disposableComponent.attachedEntities.Count > 0)
            {
                using (var iterator = disposableComponent.attachedEntities.GetEnumerator())
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