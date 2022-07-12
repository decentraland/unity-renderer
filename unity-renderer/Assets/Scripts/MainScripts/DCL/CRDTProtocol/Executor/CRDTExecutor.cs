using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;

namespace DCL.CRDT
{
    public class CRDTExecutor : ICRDTExecutor
    {
        private readonly IParcelScene ownerScene;
        private readonly ECSComponentsFactory ecsComponentsFactory;

        private ECSComponentsManager ecsManager;

        public CRDTProtocol crdtProtocol { get; }

        public CRDTExecutor(IParcelScene scene)
        {
            ownerScene = scene;
            crdtProtocol = new CRDTProtocol();
            ecsComponentsFactory = DataStore.i.ecs7.componentsFactory;
        }

        public void Dispose()
        {
            DataStore.i.ecs7.componentsManagers.Remove(ownerScene);
        }

        public void Execute(CRDTMessage crdtMessage)
        {
            CRDTMessage storedMessage = crdtProtocol.GetState(crdtMessage.key);
            CRDTMessage resultMessage = crdtProtocol.ProcessMessage(crdtMessage);

            // messages are the same so state didn't change
            if (storedMessage == resultMessage)
            {
                return;
            }

            long entityId = CRDTUtils.EntityIdFromKey(resultMessage.key);
            int componentId = CRDTUtils.ComponentIdFromKey(resultMessage.key);

            // null data means to remove component, not null data means to update or create
            if (resultMessage.data != null)
            {
                PutComponent(ownerScene, entityId, componentId, resultMessage.data);
            }
            else
            {
                RemoveComponent(ownerScene, entityId, componentId);
            }
        }

        private void PutComponent(IParcelScene scene, long entityId, int componentId, object data)
        {
            IDCLEntity entity = GetOrCreateEntity(scene, entityId);
            ECSComponentsManager ecsManager = GetOrCreateECSManager(scene);
            ecsManager.DeserializeComponent(componentId, entity, data);
        }

        private void RemoveComponent(IParcelScene scene, long entityId, int componentId)
        {
            IDCLEntity entity = scene.GetEntityById(entityId);

            if (entity == null || ecsManager == null)
            {
                return;
            }

            ecsManager.RemoveComponent(componentId, entity);

            // there is no component for this entity so we remove it
            // from scene
            if (!ecsManager.HasAnyComponent(entity))
            {
                RemoveEntity(scene, entityId);
            }
        }

        private IDCLEntity GetOrCreateEntity(IParcelScene scene, long entityId)
        {
            IDCLEntity entity = scene.GetEntityById(entityId);
            if (entity != null)
            {
                return entity;
            }

            entity = scene.CreateEntity(entityId);
            entity.OnRemoved += OnEntityRemoved;
            return entity;
        }

        private void RemoveEntity(IParcelScene scene, long entityId)
        {
            IDCLEntity entity = scene.GetEntityById(entityId);
            if (entity == null)
            {
                return;
            }

            // we unsubscribe since this methods means that entity has no components
            // so there is no need to try to clean them up on removed event
            entity.OnRemoved -= OnEntityRemoved;
            scene.RemoveEntity(entityId);
        }

        private ECSComponentsManager GetOrCreateECSManager(IParcelScene scene)
        {
            if (ecsManager != null)
            {
                return ecsManager;
            }
            ecsManager = new ECSComponentsManager(scene, ecsComponentsFactory.componentBuilders);
            DataStore.i.ecs7.componentsManagers.Add(scene, ecsManager);
            return ecsManager;
        }

        private void OnEntityRemoved(IDCLEntity entity)
        {
            ecsManager?.RemoveAllComponents(entity);
        }
    }
}