using System.Collections.Generic;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCL.ECS7
{
    public class ECS7Plugin : IPlugin
    {
        private readonly ComponentCrdtWriteSystem crdtWriteSystem;
        private readonly IECSComponentWriter componentWriter;
        private readonly ECS7ComponentsComposer componentsComposer;
        private readonly ECSSystemsController systemsController;
        private readonly ECSComponentsFactory componentsFactory;
        private readonly ECSComponentsManager componentsManager;
        private readonly InternalECSComponents internalEcsComponents;
        private readonly CrdtExecutorsManager crdtExecutorsManager;

        public ECS7Plugin()
        {
            DataStore.i.ecs7.isEcs7Enabled = true;

            ISceneController sceneController = Environment.i.world.sceneController;
            Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>(10);

            componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory);
            crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController,
                Environment.i.world.state, DataStore.i.rpc.context.crdt);

            crdtWriteSystem = new ComponentCrdtWriteSystem(Environment.i.world.state, sceneController, DataStore.i.rpc.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            componentsComposer = new ECS7ComponentsComposer(componentsFactory, componentWriter, internalEcsComponents);

            SystemsContext systemsContext = new SystemsContext(componentWriter,
                internalEcsComponents,
                new ComponentGroups(componentsManager),
                (ECSComponent<PBPointerEvents>)componentsManager.GetOrCreateComponent(ComponentID.POINTER_EVENTS),
                (ECSComponent<PBBillboard>)componentsManager.GetOrCreateComponent(ComponentID.BILLBOARD));

            systemsController = new ECSSystemsController(crdtWriteSystem.LateUpdate, systemsContext);
        }

        public void Dispose()
        {
            componentsComposer.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
            systemsController.Dispose();
            internalEcsComponents.Dispose();
            crdtExecutorsManager.Dispose();
        }
    }
}
