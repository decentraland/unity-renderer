using DCL.Controllers;
using DCL.ECS7;
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
                Debug.LogWarning($"CRDTExecutor::ExecuteWithoutStoringState Called while disposed scene {ownerScene.sceneData.sceneNumber.ToString()}");
#endif

            if (disposed)
                return;

            // null data means to remove component, not null data means to update or create
            if (data != null)
            {
                DeserializeComponent(ownerScene, entityId, componentId, data);
            }
            else
            {
                RemoveComponent(entityId, componentId);
            }
        }

        public void GenerateInitialEntities()
        {
            var cameraEntity = GetOrCreateEntity(ownerScene, SpecialEntityId.CAMERA_ENTITY);
            var playerEntity = GetOrCreateEntity(ownerScene, SpecialEntityId.PLAYER_ENTITY);
            ecsManager.GetOrCreateComponent(ComponentID.TRANSFORM, ownerScene, cameraEntity);
            ecsManager.GetOrCreateComponent(ComponentID.TRANSFORM, ownerScene, playerEntity);
        }

        public void PutComponent<T>(long entityId, ECSComponent<T> component, T model)
        {
#if UNITY_EDITOR
            if (disposed)
                Debug.LogWarning($"CRDTExecutor::PutComponent Called while disposed scene {ownerScene.sceneData.sceneNumber.ToString()}");
#endif

            if (disposed)
                return;

            IDCLEntity entity = GetOrCreateEntity(ownerScene, entityId);

            if (!component.HasComponent(ownerScene, entity))
            {
                component.Create(ownerScene, entity);
                ecsManager.SignalComponentCreated(ownerScene, entity, component);
            }

            component.SetModel(ownerScene, entity, model);
            ecsManager.SignalComponentUpdated(ownerScene, entity, component);
        }

        public void RemoveComponent(long entityId, int componentId)
        {
#if UNITY_EDITOR
            if (disposed)
                Debug.LogWarning($"CRDTExecutor::RemoveComponent Called while disposed scene {ownerScene.sceneData.sceneNumber.ToString()}");
#endif

            if (disposed)
                return;

            if (!ownerScene.entities.TryGetValue(entityId, out IDCLEntity entity))
            {
                return;
            }

            ecsManager.RemoveComponent(componentId, ownerScene, entity);

            // there is no component for this entity so we remove it
            // from scene
            if (!ecsManager.HasAnyComponent(ownerScene, entity))
            {
                RemoveEntity(ownerScene, entityId);
            }
        }

        private void DeserializeComponent(IParcelScene scene, long entityId, int componentId, object data)
        {
            IDCLEntity entity = GetOrCreateEntity(scene, entityId);
            ecsManager.DeserializeComponent(componentId, scene, entity, data);
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
