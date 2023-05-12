using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class SmartItemComponent : BaseComponent
    {
        public class Model : BaseModel
        {
            public override BaseModel GetDataFromJSON(string json) =>
                JsonConvert.DeserializeObject<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel) =>
                pbModel.PayloadCase == ComponentBodyPayload.PayloadOneofCase.SmartItem
                    ? new Model()
                    : Utils.SafeUnimplemented<SmartItemComponent, Model>(expected: ComponentBodyPayload.PayloadOneofCase.SmartItem, actual: pbModel.PayloadCase);
        }

        public override void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            base.Initialize(scene, entity);

            DataStore.i.sceneWorldObjects.AddExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);
        }

        public override void Cleanup()
        {
            DataStore.i.sceneWorldObjects.RemoveExcludedOwner(scene.sceneData.sceneNumber, entity.entityId);
            base.Cleanup();
        }

        private void Awake() { model = new Model(); }

        public override IEnumerator ApplyChanges(BaseModel newModel) { yield break; }

        public override int GetClassId() =>
            (int) CLASS_ID_COMPONENT.SMART_ITEM;

        public override string componentName => "smartItem";
    }
}
