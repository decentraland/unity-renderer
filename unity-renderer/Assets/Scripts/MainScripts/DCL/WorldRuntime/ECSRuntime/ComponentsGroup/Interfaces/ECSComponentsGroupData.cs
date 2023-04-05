using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public readonly struct ECSComponentsGroupData<TD1>
    {
        public readonly IECSReadOnlyComponentData<TD1> componentData;
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;

        public ECSComponentsGroupData(IParcelScene scene, IDCLEntity entity,
            IECSReadOnlyComponentData<TD1> componentData)
        {
            this.scene = scene;
            this.entity = entity;
            this.componentData = componentData;
        }
    }

    public readonly struct ECSComponentsGroupData<TD1, TD2>
    {
        public readonly IECSReadOnlyComponentData<TD1> componentData1;
        public readonly IECSReadOnlyComponentData<TD2> componentData2;
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;

        public ECSComponentsGroupData(IParcelScene scene, IDCLEntity entity,
            IECSReadOnlyComponentData<TD1> componentData1,
            IECSReadOnlyComponentData<TD2> componentData2)
        {
            this.scene = scene;
            this.entity = entity;
            this.componentData1 = componentData1;
            this.componentData2 = componentData2;
        }
    }

    public readonly struct ECSComponentsGroupData<TD1, TD2, TD3>
    {
        public readonly IECSReadOnlyComponentData<TD1> componentData1;
        public readonly IECSReadOnlyComponentData<TD2> componentData2;
        public readonly IECSReadOnlyComponentData<TD3> componentData3;
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;

        public ECSComponentsGroupData(IParcelScene scene, IDCLEntity entity,
            IECSReadOnlyComponentData<TD1> componentData1,
            IECSReadOnlyComponentData<TD2> componentData2,
            IECSReadOnlyComponentData<TD3> componentData3)
        {
            this.scene = scene;
            this.entity = entity;
            this.componentData1 = componentData1;
            this.componentData2 = componentData2;
            this.componentData3 = componentData3;
        }
    }
}
