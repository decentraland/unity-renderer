using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7
{
    public class ECS7Plugin : IPlugin
    {
        private readonly ComponentCrdtWriteSystem crdtWriteSystem;
        private readonly IECSComponentWriter componentWriter;
        private readonly ECS7ComponentsComposer componentsComposer;
        private readonly ECSSystemsController systemsController;
        private readonly ECSComponentsFactory componentsFactory;
        private readonly InternalECSComponents internalEcsComponents;
        private readonly CrdtExecutorsManager crdtExecutorsManager;

        internal readonly ECSComponentsManager componentsManager;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly BaseDictionary<int, Bounds> scenesOuterBounds;
        private readonly BaseDictionary<int, HashSet<Vector2Int>> scenesHashSetParcels;
        private readonly ISceneController sceneController;

        public ECS7Plugin()
        {
            DataStore.i.ecs7.isEcs7Enabled = true;
            loadedScenes = DataStore.i.ecs7.scenes;
            scenesOuterBounds = DataStore.i.ecs7.scenesOuterBounds;
            scenesHashSetParcels = DataStore.i.ecs7.scenesHashsetParcels;

            sceneController = Environment.i.world.sceneController;
            Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>(10);
            DataStore.i.rpc.context.crdt.CrdtExecutors = crdtExecutors;

            componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory);

            crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController, DataStore.i.rpc.context.crdt);

            crdtWriteSystem = new ComponentCrdtWriteSystem(crdtExecutors, sceneController, DataStore.i.rpc.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            componentsComposer = new ECS7ComponentsComposer(componentsFactory, componentWriter, internalEcsComponents);

            SystemsContext systemsContext = new SystemsContext(componentWriter,
                internalEcsComponents,
                new ComponentGroups(componentsManager),
                (ECSComponent<PBPointerEvents>)componentsManager.GetOrCreateComponent(ComponentID.POINTER_EVENTS),
                (ECSComponent<PBBillboard>)componentsManager.GetOrCreateComponent(ComponentID.BILLBOARD));

            systemsController = new ECSSystemsController(crdtWriteSystem.LateUpdate, systemsContext);

            sceneController.OnNewSceneAdded += SceneControllerOnNewSceneAdded;
            sceneController.OnSceneRemoved += SceneControllerOnSceneRemoved;
        }

        public void Dispose()
        {
            componentsComposer.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
            systemsController.Dispose();
            internalEcsComponents.Dispose();
            crdtExecutorsManager.Dispose();

            sceneController.OnNewSceneAdded -= SceneControllerOnNewSceneAdded;
            sceneController.OnSceneRemoved -= SceneControllerOnSceneRemoved;
        }

        private void SceneControllerOnNewSceneAdded(IParcelScene scene)
        {
            if (scene.sceneData.sdk7)
            {
                loadedScenes.Add(scene);
                scenesOuterBounds.Add(scene.sceneData.sceneNumber, UtilsScene.CalculateOuterBounds(scene.sceneData.parcels, scene.GetSceneTransform().position));

                HashSet<Vector2Int> hashSetParcels = new HashSet<Vector2Int>();
                for (int i = 0; i < scene.sceneData.parcels.Length; i++)
                {
                    hashSetParcels.Add(scene.sceneData.parcels[i]);
                }
                scenesHashSetParcels.Add(scene.sceneData.sceneNumber, hashSetParcels);
            }
        }

        private void SceneControllerOnSceneRemoved(IParcelScene scene)
        {
            if (scene.sceneData.sdk7)
            {
                loadedScenes.Remove(scene);
                scenesOuterBounds.Remove(scene.sceneData.sceneNumber);
                scenesHashSetParcels.Remove(scene.sceneData.sceneNumber);
            }
        }
    }
}
