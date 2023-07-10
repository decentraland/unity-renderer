using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public readonly struct ECSComponentsGroupData<TD1>
    {
        public readonly ECSComponentData<TD1> componentData;
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;

        public ECSComponentsGroupData(IParcelScene scene, IDCLEntity entity,
            in ECSComponentData<TD1> componentData)
        {
            this.scene = scene;
            this.entity = entity;
            this.componentData = componentData;
        }
    }

    public readonly struct ECSComponentsGroupData<TD1, TD2>
    {
        public readonly ECSComponentData<TD1> componentData1;
        public readonly ECSComponentData<TD2> componentData2;
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;

        public ECSComponentsGroupData(IParcelScene scene, IDCLEntity entity,
            in ECSComponentData<TD1> componentData1,
            in ECSComponentData<TD2> componentData2)
        {
            this.scene = scene;
            this.entity = entity;
            this.componentData1 = componentData1;
            this.componentData2 = componentData2;
        }

        public ECSComponentsGroupData<TD1, TD2> With(ECSComponentData<TD1> data)
        {
            return new ECSComponentsGroupData<TD1, TD2>(scene, entity, data, componentData2);
        }

        public ECSComponentsGroupData<TD1, TD2> With(ECSComponentData<TD2> data)
        {
            return new ECSComponentsGroupData<TD1, TD2>(scene, entity, componentData1, data);
        }
    }

    public readonly struct ECSComponentsGroupData<TD1, TD2, TD3>
    {
        public readonly ECSComponentData<TD1> componentData1;
        public readonly ECSComponentData<TD2> componentData2;
        public readonly ECSComponentData<TD3> componentData3;
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;

        public ECSComponentsGroupData(IParcelScene scene, IDCLEntity entity,
            in ECSComponentData<TD1> componentData1,
            in ECSComponentData<TD2> componentData2,
            in ECSComponentData<TD3> componentData3)
        {
            this.scene = scene;
            this.entity = entity;
            this.componentData1 = componentData1;
            this.componentData2 = componentData2;
            this.componentData3 = componentData3;
        }

        public ECSComponentsGroupData<TD1, TD2, TD3> With(ECSComponentData<TD1> data)
        {
            return new ECSComponentsGroupData<TD1, TD2, TD3>(scene, entity, data, componentData2, componentData3);
        }

        public ECSComponentsGroupData<TD1, TD2, TD3> With(ECSComponentData<TD2> data)
        {
            return new ECSComponentsGroupData<TD1, TD2, TD3>(scene, entity, componentData1, data, componentData3);
        }

        public ECSComponentsGroupData<TD1, TD2, TD3> With(ECSComponentData<TD3> data)
        {
            return new ECSComponentsGroupData<TD1, TD2, TD3>(scene, entity, componentData1, componentData2, data);
        }
    }
}
