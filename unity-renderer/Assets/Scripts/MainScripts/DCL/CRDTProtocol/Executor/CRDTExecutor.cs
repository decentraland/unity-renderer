using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;

namespace DCL.CRDT
{
    public class CRDTExecutor : ICRDTExecutor
    {
        private readonly IParcelScene ownerScene;
        private readonly ECSComponentsManager ecsManager;

        private bool sceneAdded = false;
        private bool disposed = false;
        private readonly IList<IParcelScene> loadedScenes;

        public CRDTProtocol crdtProtocol { get; }

        public CRDTExecutor(IParcelScene scene, ECSComponentsManager componentsManager)
        {
            ownerScene = scene;
            crdtProtocol = new CRDTProtocol();
            ecsManager = componentsManager;
            loadedScenes = DataStore.i.ecs7.scenes;
        }

        public void Dispose()
        {
            disposed = true;
            loadedScenes.Remove(ownerScene);
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

        public void Execute(CRDTMessage crdtMessage)
        {
            if (!sceneAdded)
            {
                sceneAdded = true;
                loadedScenes.Add(ownerScene);
            }

            CRDTMessage storedMessage = crdtProtocol.GetState(crdtMessage.key1, crdtMessage.key2);
            CRDTMessage resultMessage = crdtProtocol.ProcessMessage(crdtMessage);

            // messages are the same so state didn't change
            if (storedMessage == resultMessage)
            {
                return;
            }
            

            ExecuteWithoutStoringState(crdtMessage.key1, crdtMessage.key2, crdtMessage.data);
        }

        public void ExecuteWithoutStoringState(long entityId, int componentId, object data)
        {
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