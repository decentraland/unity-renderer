using DCL.ECS7.InternalComponents;

namespace ECSSystems.IncreaseSceneTickSystem
{
    public class IncreaseSceneTickSystem
    {
        private readonly IInternalECSComponent<InternalIncreaseTickTagComponent> increaseSceneTickTagComponent;
        private readonly IInternalECSComponent<InternalEngineInfo> engineInfoComponent;

        public IncreaseSceneTickSystem(
            IInternalECSComponent<InternalIncreaseTickTagComponent> increaseSceneTickTagComponent,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent)
        {
            this.engineInfoComponent = engineInfoComponent;
            this.increaseSceneTickTagComponent = increaseSceneTickTagComponent;
        }

        public void Update()
        {
            var increaseTickComponents = increaseSceneTickTagComponent.GetForAll();

            for (int i = 0; i < increaseTickComponents.Count; i++)
            {
                var value = increaseTickComponents[i].value;

                if (!value.model.dirty)
                    continue;

                var engineInfoData = engineInfoComponent.GetFor(value.scene, value.entity);

                if (engineInfoData != null)
                {
                    engineInfoData.model.SceneTick++;
                    engineInfoComponent.PutFor(engineInfoData.scene, engineInfoData.entity, engineInfoData.model);
                }
            }
        }
    }
}
