using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.CRDT
{
    public class CRDTExecutor : ICRDTExecutor
    {
        private readonly IParcelScene ownerScene;
        private readonly ECSComponentsManager ecsManager;

        private bool disposed = false;

        public CRDTProtocol crdtProtocol { get; }

        public CRDTExecutor(IParcelScene scene, ECSComponentsManager componentsManager)
        {
            ownerScene = scene;
            crdtProtocol = new CRDTProtocol();
            ecsManager = componentsManager;
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (disposed)
                Debug.LogWarning("CRDTExecutor::Dispose Called while disposed");
#endif

            if (disposed)
                return;

            disposed = true;

            using (var entities = ownerScene.entities.Values.GetEnumerator())
            {
                while (entities.MoveNext())
                {
                    var entity = entities.Current;
                    entity.OnRemoved -= OnEntityRemoved;
                    ecsManager.RemoveAllComponents(ownerScene, entity);
                }
            }
        }

        public void Execute(CrdtMessage crdtMessage)
        {
#if UNITY_EDITOR
            if (disposed)
                Debug.LogWarning("CRDTExecutor::Execute Called while disposed");
#endif

            CRDTProtocol.ProcessMessageResultType resultType = crdtProtocol.ProcessMessage(crdtMessage);

            // If the message change the state
            if (resultType == CRDTProtocol.ProcessMessageResultType.StateUpdatedData ||
                resultType == CRDTProtocol.ProcessMessageResultType.StateUpdatedTimestamp ||
                resultType == CRDTProtocol.ProcessMessageResultType.EntityWasDeleted)
            {
                ExecuteWithoutStoringState(crdtMessage.EntityId, crdtMessage.ComponentId, crdtMessage.Data);
            }
        }

        public void ExecuteWithoutStoringState(long entityId, int componentId, object data)
        {
#if UNITY_EDITOR
            if (disposed)
                Debug.LogWarning("CRDTExecutor::ExecuteWithoutStoringState Called while disposed");
#endif

            if (disposed)
                return;

            // null data means to remove component, not null data means to update or create
            if (data != null)
            {
                PutComponent(ownerScene, entityId, componentId, data);
            }
            else
            {
                RemoveComponent(ownerScene, entityId, componentId);
            }
        }

        private void PutComponent(IParcelScene scene, long entityId, int componentId, object data)
        {
            IDCLEntity entity = GetOrCreateEntity(scene, entityId);
            ecsManager.DeserializeComponent(componentId, scene, entity, data);
        }

        private void RemoveComponent(IParcelScene scene, long entityId, int componentId)
        {
            if (!scene.entities.TryGetValue(entityId, out IDCLEntity entity))
            {
                return;
            }

            ecsManager.RemoveComponent(componentId, scene, entity);

            // there is no component for this entity so we remove it
            // from scene
            if (!ecsManager.HasAnyComponent(scene, entity))
            {
                RemoveEntity(scene, entityId);
            }
        }

        private IDCLEntity GetOrCreateEntity(IParcelScene scene, long entityId)
        {
            if (scene.entities.TryGetValue(entityId, out IDCLEntity entity))
            {
                return entity;
            }

            // CreateEntity internally adds entity to `scene.entities`
            entity = scene.CreateEntity(entityId);
            entity.OnRemoved += OnEntityRemoved;
            return entity;
        }

        private void RemoveEntity(IParcelScene scene, long entityId)
        {
            if (!scene.entities.TryGetValue(entityId, out IDCLEntity entity))
            {
                return;
            }

            // we unsubscribe since this methods means that entity has no components
            // so there is no need to try to clean them up on removed event
            entity.OnRemoved -= OnEntityRemoved;
            scene.RemoveEntity(entityId);
        }

        private void OnEntityRemoved(IDCLEntity entity)
        {
            entity.OnRemoved -= OnEntityRemoved;
            ecsManager.RemoveAllComponents(ownerScene, entity);
        }
    }
}
